using Application.Extensions;

namespace Application.Interfaces.Services
{
    public interface IAiPipelineService
    {
        Task<AiResult> ProcessAsync(string filePath);
    }
}
