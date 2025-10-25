using Microsoft.AspNetCore.Mvc;
using TodoList.Services;

namespace TodoList.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SyncController : ControllerBase
    {
        private readonly ISyncService _syncService;

        public SyncController(ISyncService syncService)
        {
            _syncService = syncService;
        }

        [HttpPost]
        public async Task<ActionResult> SyncToDoList()
        {
            var url = "https://jsonplaceholder.typicode.com/todos";
            try
            {
                await _syncService.SyncData(url);
                return Ok("Registros sincronizados.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro na sincronização: {ex.Message}");
            }
        }
    }
}
