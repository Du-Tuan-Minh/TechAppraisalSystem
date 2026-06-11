using Microsoft.AspNetCore.Mvc;

namespace TechAppraisalSystem.Controllers
{
    [ApiController]
    [Route("api/ai-ocr")]
    public class OcrController : ControllerBase
    {
        //private readonly IOcrService _ocrService;
        //public OcrController(IOcrService ocrService) => _ocrService = ocrService;

        //[HttpPost("extract-specs")]
        //public async Task<IActionResult> Extract([FromForm] IFormFile file)
        //{
        //    if (file == null) return BadRequest("Không có file.");

        //    using var stream = file.OpenReadStream();
        //    var result = await _ocrService.ExtractTechnicalSpecsAsync(stream);

        //    return Ok(result);
        //}
    }
}
