using KcbBot.EchoBot.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KcbBot.EchoBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly IStore _MemoryStore;

        public DebugController(IStore memoryStore)
        {
            _MemoryStore = memoryStore;
        }

        [HttpGet("store")]
        public IActionResult GetMemoryStore()
        {
            // Return the store contents as JSON
            return Ok(_MemoryStore.GetAll()); // Assuming you have a method to retrieve all items
        }
    }
}
