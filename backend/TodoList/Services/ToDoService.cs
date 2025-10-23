using Microsoft.EntityFrameworkCore;
using TodoList.Data;
using TodoList.Models.Entities;

namespace TodoList.Services
{
    public class ToDoService
    {
        private readonly ApplicationDbContext _dbContext;

        public ToDoService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<object> GetAll(int pageNumber = 1, int pageSize = 10, string? sortBy = "Id", bool isDescending = false, string? title = null)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

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

        public async Task<ToDo?> GetById(int id)
        {
            return await _dbContext.ToDos
                .Include(t => t.User)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

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

        public async Task<ToDo> Add(ToDo newToDo)
        {
            if (newToDo == null) throw new ArgumentNullException(nameof(newToDo));

            _dbContext.ToDos.Add(newToDo);
            await _dbContext.Database.OpenConnectionAsync();
            try
            {
                await _dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.ToDos ON");
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.ToDos OFF");
            }
            finally
            {
                await _dbContext.Database.CloseConnectionAsync();
            }

            return newToDo;
        }
    }
}
