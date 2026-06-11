using Application.Common;
using Application.DTOs;
using Domain.Enums;

namespace Application.Interfaces.Services
{
    public interface IAiService
    {
        Task<ApiResponse<string>> GetAiSuggestionTemplateAsync(DocumentType type, string keyword);
        Task<ApiResponse<IEnumerable<FeedbackIssueResponseDto>>> RunAiCrossCheckAsync(Guid documentId);
    }
}