using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace TechAppraisalSystem.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly IAiPipelineService _service;

        public AiController(IAiPipelineService service)
        {
            _service = service;
        }

        [HttpPost("process")]
        public async Task<IActionResult> Process(IFormFile file)
        {
            var path = Path.GetTempFileName();

            using (var stream = System.IO.File.Create(path))
            {
                await file.CopyToAsync(stream);
            }

            var result = await _service.ProcessAsync(path);

            return Ok(result);
        }
    }
}
