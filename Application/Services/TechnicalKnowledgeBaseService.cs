using Application.Common;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using System.Text.Json;

public class TechnicalKnowledgeBaseService : BaseService, ITechnicalKnowledgeBaseService
{
    private readonly IMapper _mapper;

    public TechnicalKnowledgeBaseService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
    {
        _mapper = mapper;
    }

    //public async Task<ApiResponse<PagedResult<TechnicalKnowledgeBaseResponseDto>>> GetListAsync(KnowledgeBaseFilterDto filter)
    //{
    //    string? jsonKey = null, jsonValue = null;
    //    if (filter.SearchTerm?.Contains(":") == true)
    //    {
    //        var parts = filter.SearchTerm.Split(':');
    //        jsonKey = parts[0];
    //        jsonValue = parts[1];
    //    }

    //    var pagedEntities = await _unitOfWork.TechnicalKnowledgeBase.GetFilteredAsync(
    //        filter.SearchTerm, jsonKey, filter.Page, filter.PageSize);

    //    var dtos = _mapper.Map<List<TechnicalKnowledgeBaseResponseDto>>(pagedEntities.Items);

    //    foreach (var dto in dtos)
    //    {
    //        var entity = pagedEntities.Items.First(x => x.Id == dto.Id);
    //        dto.LinkedSpecPattern = JsonSerializer.Deserialize<object>(entity.LinkedSpecPattern);
    //    }

    //    return ApiResponse<PagedResult<TechnicalKnowledgeBaseResponseDto>>.Success(new PagedResult<TechnicalKnowledgeBaseResponseDto>
    //    {
    //        Items = dtos,
    //        TotalCount = pagedEntities.TotalCount,
    //        Page = pagedEntities.Page,
    //        PageSize = pagedEntities.PageSize
    //    });
    //}

    public async Task<ApiResponse<PagedResult<TechnicalKnowledgeBaseResponseDto>>> GetListAsync(KnowledgeBaseFilterDto filter)
    {
        var pagedEntities = await _unitOfWork.TechnicalKnowledgeBase.GetFilteredAsync(
            filter.SearchTerm,
            filter.Page,
            filter.PageSize);

        var dtos = _mapper.Map<List<TechnicalKnowledgeBaseResponseDto>>(pagedEntities.Items);

        return ApiResponse<PagedResult<TechnicalKnowledgeBaseResponseDto>>.Success(
            new PagedResult<TechnicalKnowledgeBaseResponseDto>
            {
                Items = dtos,
                TotalCount = pagedEntities.TotalCount,
                Page = pagedEntities.Page,
                PageSize = pagedEntities.PageSize
            });
    }

    public async Task<ApiResponse<TechnicalKnowledgeBaseDetailDto>> GetDetailAsync(Guid id, string? searchTerm)
    {
        string? jsonKey = null;

        if (searchTerm?.Contains(":") == true)
        {
            var parts = searchTerm.Split(':');
            jsonKey = parts[0];
        }

        var entity = await _unitOfWork.TechnicalKnowledgeBase.GetDetailAsync(id, searchTerm, jsonKey);
        if (entity == null) return ApiResponse<TechnicalKnowledgeBaseDetailDto>.Failure(404, "Không tìm thấy");

        var dto = _mapper.Map<TechnicalKnowledgeBaseDetailDto>(entity);

        return ApiResponse<TechnicalKnowledgeBaseDetailDto>.Success(dto);
    }


    public async Task<ApiResponse<Guid>> CreateAsync(TechnicalKnowledgeBaseCreateDto dto)
    {
        var entity = _mapper.Map<TechnicalKnowledgeBase>(dto);
        entity.LinkedSpecPattern = JsonSerializer.Serialize(dto.LinkedSpecPattern);

        await _unitOfWork.TechnicalKnowledgeBase.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<Guid>.Success(entity.Id);
    }

    public async Task<ApiResponse<bool>> SoftDeleteAsync(Guid id)
    {
        var entity = await _unitOfWork.TechnicalKnowledgeBase.GetByIdAsync(id);
        if (entity == null) return ApiResponse<bool>.Failure(404, "Không tìm thấy.");

        entity.IsDeleted = true;
        _unitOfWork.TechnicalKnowledgeBase.Update(entity);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<List<SuggestionResponseDto>>> GetSmartSuggestionsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return ApiResponse<List<SuggestionResponseDto>>.Success(new());

        var entities = await _unitOfWork.TechnicalKnowledgeBase.GetSuggestionsByProductAsync(searchTerm);
        if (!entities.Any()) return ApiResponse<List<SuggestionResponseDto>>.Success(new());

        var entityIds = entities.Select(e => e.Id).ToList();

        var attachmentLinks = await _unitOfWork.AttachmentLinks.GetLinksByEntityIdsAsync(
            entityIds, LinkedEntityType.TechnicalKnowledgeBase);

        var linksLookup = attachmentLinks.ToLookup(al => al.EntityId);
        var suggestions = new List<SuggestionResponseDto>();

        foreach (var entity in entities)
        {
            var suggestion = _mapper.Map<SuggestionResponseDto>(entity);
            var linksForEntity = linksLookup[entity.Id];

            suggestion.Attachments = _mapper.Map<List<AttachmentShortDto>>(linksForEntity);
            suggestions.Add(suggestion);
        }

        return ApiResponse<List<SuggestionResponseDto>>.Success(suggestions);
    }

    public async Task<ApiResponse<(Stream Stream, string FileType, string FileName)>> DownloadFileAsync(Guid id)
    {
        var attachment = await _unitOfWork.Attachments.GetByIdAsync(id);

        if (attachment == null) return ApiResponse<(Stream, string, string)>.Failure(404, "Tài liệu không tồn tại");
        if (attachment.FileData == null || attachment.FileData.Length == 0) return ApiResponse<(Stream, string, string)>.Failure(404, "Dữ liệu file bị trống");

        var stream = new MemoryStream(attachment.FileData);
        stream.Position = 0;

        return ApiResponse<(Stream, string, string)>.Success((stream, attachment.FileType, attachment.FileName));
    }
}