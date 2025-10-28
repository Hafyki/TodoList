using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Linq;
using TodoList.Data;
using TodoList.DTO;
using TodoList.Models.Entities;

namespace TodoList.Services
{
    public class ToDoService : IToDoService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IUserService _userService;

        public ToDoService(ApplicationDbContext dbContext, IUserService userService)
        {
            _dbContext = dbContext;
            _userService = userService;
        }

        //GET ALL
        public async Task<object> GetAll(int pageNumber = 1, int pageSize = 10, string? sortBy = "Id", bool isDescending = false, string? title = null)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 10) pageSize = 10;
            IQueryable<ToDo> query = _dbContext.ToDos.AsQueryable();

            query = sortBy?.ToLower() switch
            {
                "title" => isDescending ? query.OrderByDescending(q => q.Title) : query.OrderBy(q => q.Title),
                "completed" => isDescending ? query.OrderByDescending(q => q.IsCompleted) : query.OrderBy(q => q.IsCompleted),
                "id" => isDescending ? query.OrderByDescending(q => q.Id) : query.OrderBy(q => q.Id),
                _ => query.OrderBy(q => q.Id)
            };

            if (!string.IsNullOrEmpty(title))
                query = query.Where(q => q.Title.Contains(title));

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(t => t.User)
                .ToListAsync();

            return new
            {
                pageNumber,
                pageSize,
                totalItems,
                totalPages,
                items
            };
        }
        //GET BY ID
        public async Task<ToDo?> GetById(int id)
        {
            return await _dbContext.ToDos
                .Include(t => t.User)
                .FirstOrDefaultAsync(i => i.Id == id);
        }
        //PUT - TROCAR STATUS DA TAREFA
        public async Task<ToDo?> ChangeCompletedStatus(int id)
        {
            var todo = await _dbContext.ToDos.FindAsync(id);
            if (todo == null) return null;

            //Regra de negócio
            if (todo.IsCompleted)
            {
                var incompletedTasks = await _dbContext.ToDos
            .Where(t => t.UserId == todo.UserId && !t.IsCompleted)
            .CountAsync();
                if (incompletedTasks >= 5)
                {
                    throw new InvalidOperationException($"O usuário de id:{todo.UserId} já possui 5 tarefas incompletas. Não é possível marcar mais tarefas como pendentes.");
                }
            }

            todo.IsCompleted = !todo.IsCompleted;
            _dbContext.ToDos.Update(todo);
            await _dbContext.SaveChangesAsync();
            return todo;
        }
        //POST
        public async Task<ToDo> Add(ToDoDto newToDoDto)
        {
            if (newToDoDto == null)
                throw new ArgumentNullException(nameof(newToDoDto));

            // Verifica se o usuário existe
            var user = await _dbContext.Users.FindAsync(newToDoDto.UserId);
            if (user == null)
                throw new InvalidOperationException($"Usuário com ID {newToDoDto.UserId} não encontrado.");

            // Verifica se já existe um ToDo com o mesmo Id
            var existingToDo = await _dbContext.ToDos.FindAsync(newToDoDto.Id);
            if (existingToDo != null)
                throw new InvalidOperationException($"Já existe uma tarefa com o ID {newToDoDto.Id}.");

            // Cria a entidade a partir do DTO
            var newToDoEntity = new ToDo
            {
                Id = newToDoDto.Id, // ← Permite ID manual
                Title = newToDoDto.Title,
                IsCompleted = newToDoDto.IsCompleted,
                UserId = newToDoDto.UserId,
                User = user
            };

            // Abre transação manual apenas porque usaremos IDENTITY_INSERT
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Permite inserir valor no campo identity
                await _dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.ToDos ON;");

                _dbContext.ToDos.Add(newToDoEntity);
                await _dbContext.SaveChangesAsync();

                await _dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.ToDos OFF;");

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return newToDoEntity;
        }

    }
}