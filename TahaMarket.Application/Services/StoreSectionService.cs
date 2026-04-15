using Microsoft.EntityFrameworkCore;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services.Common;
using TahaMarket.Domain.Entities;
using TahaMarket.Infrastructure.Data;

public class StoreSectionService
{
    private readonly ApplicationDbContext _context;
    private readonly ImageService _imageService;
    private readonly FileUrlService _fileUrl;

    public StoreSectionService(
        ApplicationDbContext context,
        ImageService imageService,
        FileUrlService fileUrl)
    {
        _context = context;
        _imageService = imageService;
        _fileUrl = fileUrl;
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

        // ⚠️ optional: check if stores exist
        var hasStores = await _context.Stores
            .AnyAsync(s => s.StoreSectionId == id);

        if (hasStores)
            throw new Exception("Cannot delete section with assigned stores");

        _context.storeSections.Remove(section);
        await _context.SaveChangesAsync();
    }
}