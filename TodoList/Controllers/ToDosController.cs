using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoList.Data;
using TodoList.Models.Entities;

namespace TodoList.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToDosController(ToDoListDbContext dbContext) : ControllerBase
    {
       private readonly ToDoListDbContext dbContext = dbContext;
        
        //GET ALL       
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToDo>>> Get(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filter = null,
            [FromQuery] string? sortBy = "Id",
            [FromQuery] bool isDescending = false)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<ToDo> query = dbContext.Quests.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(q =>q.Title.Contains(filter));
            }

            query = sortBy?.ToLower() switch
            {
                "title" => isDescending ? query.OrderByDescending(q => q.Title) : query.OrderBy(q => q.Title),
                "completed" => isDescending ? query.OrderByDescending(q => q.IsCompleted) : query.OrderBy(q => q.IsCompleted),
                "id" => isDescending ? query.OrderByDescending(q => q.Id) : query.OrderBy(q => q.Id)
            };

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var quests = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(quests);
        }

        //GET BY ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ToDo>> GetById(int id)
        {
            var quest = await dbContext.Quests.FindAsync(id);
            if (quest == null)
                return NotFound();

            return Ok(quest);
        }
        //PUT
        [HttpPut("{id}")]
        public async Task<ActionResult<ToDo>> ChangeCompletedStatus(int id)
        {
            var quest = await dbContext.Quests.FindAsync(id);
            if (quest == null)
                return NotFound();
            if (quest.IsCompleted == false)
                quest.IsCompleted = true;
            else
                quest.IsCompleted = false;

            dbContext.Quests.Update(quest);
            await dbContext.SaveChangesAsync();
            return Ok(quest);
        }
        //POST
        [HttpPost]
        public async Task<ActionResult<ToDo>> Add(ToDo newQuest)
        {
            if (newQuest == null)
                return BadRequest();
            dbContext.Quests.Add(newQuest);
            await dbContext.Database.OpenConnectionAsync();
            try
            {
                await dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Quests ON");
                await dbContext.SaveChangesAsync();
                await dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Quests OFF");
            }
            finally
            {
                await dbContext.Database.CloseConnectionAsync();
            }
            

            return CreatedAtAction(nameof(GetById), new {id = newQuest.Id}, newQuest);
        }
    }
}
