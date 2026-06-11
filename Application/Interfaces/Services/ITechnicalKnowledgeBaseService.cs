using Application.Common;
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface ITechnicalKnowledgeBaseService
    {
        Task<ApiResponse<PagedResult<TechnicalKnowledgeBaseResponseDto>>> GetListAsync(KnowledgeBaseFilterDto filter);
        Task<ApiResponse<TechnicalKnowledgeBaseDetailDto>> GetDetailAsync(Guid id, string? searchTerm);
        Task<ApiResponse<Guid>> CreateAsync(TechnicalKnowledgeBaseCreateDto dto);
        Task<ApiResponse<bool>> SoftDeleteAsync(Guid id);
        Task<ApiResponse<List<SuggestionResponseDto>>> GetSmartSuggestionsAsync(string searchTerm);
        Task<ApiResponse<(Stream Stream, string FileType, string FileName)>> DownloadFileAsync(Guid id);
    }
}
