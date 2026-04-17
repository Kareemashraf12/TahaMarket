using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services.Common;
using TahaMarket.Domain.Entities;
using TahaMarket.Domain.Enums;
using TahaMarket.Infrastructure.Data;

public class StoreSectionService
{
    private readonly ApplicationDbContext _context;
    private readonly ImageService _imageService;
    private readonly FileUrlService _fileUrl;
    private readonly IMemoryCache _cache;
    public StoreSectionService(
        ApplicationDbContext context,
        ImageService imageService,
        FileUrlService fileUrl, IMemoryCache cache)
    {
        _context = context;
        _imageService = imageService;
        _fileUrl = fileUrl;
        _cache = cache;
    }

    // =========================================
    // CREATE SECTION (ADMIN ONLY)
    // =========================================
    public async Task<StoreSectionResponse> Create(CreateStoreSectionRequest request)
    {
        if (request == null)
            throw new Exception("Invalid request");

        var imagePath = await _imageService.SaveImage(request.Image, "images/store-sections");

        var section = new StoreSection
        {
            Name = request.Name,
            ImageUrl = imagePath
        };

        _context.storeSections.Add(section);
        await _context.SaveChangesAsync();

        return new StoreSectionResponse
        {
            Id = section.Id,
            Name = section.Name,
            ImageUrl = _fileUrl.GetFullUrl(section.ImageUrl)
        };
    }


    // ==========================
    // Get Stores By Section
    // ==========================
    public async Task<PaginatedResponse<StoreResponse>> GetStoresBySection(
     Guid sectionId,
     PaginationRequest request)
    {
        // =========================
        // VALIDATION
        // =========================
        if (request.Page <= 0) request.Page = 1;
        if (request.PageSize <= 0 || request.PageSize > 50) request.PageSize = 10;

        var cacheKey = $"stores_section_{sectionId}_{request.Page}_{request.PageSize}";

        // =========================
        // CACHE
        // =========================
        if (_cache.TryGetValue(cacheKey, out PaginatedResponse<StoreResponse> cachedData))
            return cachedData;

        // =========================
        // BASE QUERY
        // =========================
        var baseQuery = _context.Stores
            .AsNoTracking()
            .Where(s => s.StoreSectionId == sectionId);

        var totalCount = await baseQuery.CountAsync();

        // =========================
        // OPTIMIZED QUERY (SQL LEVEL)
        // =========================
        var data = await baseQuery
            .OrderByDescending(s => s.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.ImageUrl,
                s.Description,

                AvgRating = _context.Ratings
                    .Where(r => r.TargetType == RatingTargetType.Store && r.TargetId == s.Id)
                    .Average(r => (double?)r.Value) ?? 0
            })
            .ToListAsync();

        // =========================
        // MAPPING (C# LEVEL)
        // =========================
        var stores = data.Select(x => new StoreResponse
        {
            Id = x.Id,
            Name = x.Name,
            ImageUrl = x.ImageUrl != null
                ? _fileUrl.GetFullUrl(x.ImageUrl)
                : null,
            Description = x.Description,
            AverageRating = x.AvgRating
        }).ToList();

        var response = new PaginatedResponse<StoreResponse>
        {
            Page = request.Page,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
            Data = stores
        };

        // =========================
        // SET CACHE (1 minute)
        // =========================
        _cache.Set(cacheKey, response, TimeSpan.FromMinutes(1));

        return response;
    }

    // =========================================
    // GET ALL SECTIONS
    // =========================================
    public async Task<List<StoreSectionResponse>> GetAll()
    {
        return await _context.storeSections
            .Select(s => new StoreSectionResponse
            {
                Id = s.Id,
                Name = s.Name,
                ImageUrl = _fileUrl.GetFullUrl(s.ImageUrl)
            })
            .ToListAsync();
    }

    // =========================================
    // GET BY ID 
    // =========================================
    public async Task<StoreSectionResponse> GetById(Guid id)
    {
        var section = await _context.storeSections
            .FirstOrDefaultAsync(s => s.Id == id);

        if (section == null)
            throw new Exception("Section not found");

        return new StoreSectionResponse
        {
            Id = section.Id,
            Name = section.Name,
            ImageUrl = _fileUrl.GetFullUrl(section.ImageUrl)
        };
    }

    // =========================================
    // DELETE SECTION
    // =========================================
    public async Task Delete(Guid id)
    {
        var section = await _context.storeSections
            .FirstOrDefaultAsync(s => s.Id == id);

        if (section == null)
            throw new Exception("Section not found");

        //  optional: check if stores exist
        var hasStores = await _context.Stores
            .AnyAsync(s => s.StoreSectionId == id);

        if (hasStores)
            throw new Exception("Cannot delete section with assigned stores");

        _context.storeSections.Remove(section);
        await _context.SaveChangesAsync();
    }
}