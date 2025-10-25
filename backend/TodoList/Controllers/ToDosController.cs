using Microsoft.AspNetCore.Mvc;
using TodoList.DTO;
using TodoList.Models.Entities;
using TodoList.Services;

namespace TodoList.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToDosController : ControllerBase
    {
        private readonly IToDoService _toDoService;

        public ToDosController(IToDoService toDoService)
        {
            _toDoService = toDoService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = "Id",
            [FromQuery] bool isDescending = false,
            [FromQuery] string? title = null
        )
        {
            var result = await _toDoService.GetAll(pageNumber, pageSize, sortBy, isDescending, title);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var todo = await _toDoService.GetById(id);
            if (todo == null) return NotFound();
            return Ok(todo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ChangeCompletedStatus(int id)
        {
            try
            {
                var todo = await _toDoService.ChangeCompletedStatus(id);
                if (todo == null) return NotFound();
                return Ok(todo);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(ToDoDto newToDo)
        {
            try
            {
                var todo = await _toDoService.Add(newToDo);
                return CreatedAtAction(nameof(GetById), new {id = todo.Id},todo);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            
        
            
        }
    }
}
