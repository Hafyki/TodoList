using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoList.Data;
using TodoList.Models.Entities;

namespace TodoList.Controllers
{
    [Route("api/todos")]
    [ApiController]
    public class ToDosController(ApplicationDbContext dbContext) : ControllerBase
    {
       private readonly ApplicationDbContext _dbContext = dbContext;
        
        //GET ALL       
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToDo>>> Get(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = "Id",
            [FromQuery] bool isDescending = false,
            [FromQuery] string? title = null
            )
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<ToDo> query = _dbContext.ToDos.AsQueryable();

            query = sortBy?.ToLower() switch
            {
                "title" => isDescending ? query.OrderByDescending(q => q.Title) : query.OrderBy(q => q.Title),
                "completed" => isDescending ? query.OrderByDescending(q => q.IsCompleted) : query.OrderBy(q => q.IsCompleted),
                "id" => isDescending ? query.OrderByDescending(q => q.Id) : query.OrderBy(q => q.Id)
            };
            
            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(q => q.Title.Contains(title));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var quests = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(t => t.User)
                .ToListAsync();
            
            var result = new
            {
                pageNumber,
                pageSize,
                totalItems,
                totalPages,
                items = quests
            };

            return Ok(result);
        }

        //GET BY ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ToDo>> GetById(int id)
        {
            var quest = await _dbContext.ToDos
                .Include(t => t.User)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (quest == null)
                return NotFound();

            return Ok(quest);
        }
        //PUT
        [HttpPut("{id}")]
        public async Task<ActionResult<ToDo>> ChangeCompletedStatus(int id)
        {
            var quest = await _dbContext.ToDos.FindAsync(id);
            if (quest == null)
                return NotFound();
            if (quest.IsCompleted == false)
                quest.IsCompleted = true;
            else
                quest.IsCompleted = false;

            _dbContext.ToDos.Update(quest);
            await _dbContext.SaveChangesAsync();
            return Ok(quest);
        }
        //POST
        [HttpPost]
        public async Task<ActionResult<ToDo>> Add(ToDo newQuest)
        {
            if (newQuest == null)
                return BadRequest();
            _dbContext.ToDos.Add(newQuest);
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
            

            return CreatedAtAction(nameof(GetById), new {id = newQuest.Id}, newQuest);
        }
    }
}
