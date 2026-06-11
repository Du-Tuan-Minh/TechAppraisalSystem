using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechAppraisalSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/knowledgeBase")]
    public class TechnicalKnowledgeBaseController : BaseApiController<TechnicalKnowledgeBaseController>
    {
        public TechnicalKnowledgeBaseController(IServiceManager services, ILogger<TechnicalKnowledgeBaseController> logger)
      : base(services, logger)
        { }

        [HttpGet("getlist")]
        public async Task<IActionResult> GetList([FromQuery] KnowledgeBaseFilterDto filter)
        {
            var result = await _serviceManager.TechnicalKnowledgeBaseService.GetListAsync(filter);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] TechnicalKnowledgeBaseCreateDto dto)
        {
            var result = await _serviceManager.TechnicalKnowledgeBaseService.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _serviceManager.TechnicalKnowledgeBaseService.SoftDeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetDetail(Guid id, [FromQuery] string? searchTerm)
        {
            var result = await _serviceManager.TechnicalKnowledgeBaseService.GetDetailAsync(id, searchTerm);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("smart-suggestions")]
        public async Task<IActionResult> GetSmartSuggestions([FromQuery] string query)
        {
            var result = await _serviceManager.TechnicalKnowledgeBaseService.GetSmartSuggestionsAsync(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(Guid id)
        {
            var result = await _serviceManager.TechnicalKnowledgeBaseService.DownloadFileAsync(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return File(result.Data.Stream, result.Data.FileType, result.Data.FileName);
        }
    }
}