using Application.Common;
using Domain.Entities;

namespace Application.Interfaces.Persistence
{
    public interface ITechnicalKnowledgeBaseRepository : IRepository<TechnicalKnowledgeBase>
    {
        Task<PagedResult<TechnicalKnowledgeBase>> GetFilteredAsync(
            string? searchTerm,
            int page,
            int pageSize);

        Task<TechnicalKnowledgeBase?> GetDetailAsync(
            Guid id,
            string? searchTerm,
            string? jsonKey);

        Task<List<TechnicalKnowledgeBase>> GetSuggestionsByProductAsync(string partNumberOrName);
    }
}