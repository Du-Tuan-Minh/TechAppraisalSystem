using Application.Common;
using Application.Extensions;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories
{
    public class TechnicalKnowledgeBaseRepository : BaseRepository<TechnicalKnowledgeBase>, ITechnicalKnowledgeBaseRepository
    {
        public TechnicalKnowledgeBaseRepository(ApplicationDbContext context, ILogger<TechnicalKnowledgeBaseRepository> logger)
            : base(context, logger) { }

        public async Task<PagedResult<TechnicalKnowledgeBase>> GetFilteredAsync(
            string? searchTerm,
            int page,
            int pageSize)
        {
            var query = _dbSet.AsNoTracking()
                .Include(x => x.Verifier)
                .Where(x => !x.IsDeleted) 
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(x => x.Title.Contains(searchTerm) || x.TechnicalSolution.Contains(searchTerm));
            }

            query = query.OrderByDescending(x => x.CreatedAt);
            return await query.ToPagedListAsync(page, pageSize);
        }

        public async Task<TechnicalKnowledgeBase?> GetDetailAsync(
            Guid id,
            string? searchTerm,
            string? jsonKey)
        {
            var query = _dbSet.AsNoTracking()
                .Include(x => x.Verifier).ThenInclude(v => v.Profile)
                .Where(x => !x.IsDeleted && x.Id == id);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(x => x.TechnicalSolution.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(jsonKey))
            {
                query = query.Where(x => EF.Functions.JsonExists(x.LinkedSpecPattern, jsonKey));
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<TechnicalKnowledgeBase>> GetSuggestionsByProductAsync(string searchTerm)
        {
            return await _dbSet.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsVerified) // Chỉ lấy dữ liệu đã thẩm định
                .Where(x => x.Title.Contains(searchTerm)) // Tìm theo PartNumber hoặc tên SP
                .OrderByDescending(x => x.OccurrenceCount) // Ưu tiên cái dùng nhiều
                .Take(5) // Lấy top 5 gợi ý
                .ToListAsync();
        }
    }
}