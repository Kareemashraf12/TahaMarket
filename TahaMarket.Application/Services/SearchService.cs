using Microsoft.EntityFrameworkCore;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services.Common;
using TahaMarket.Domain.Enums;
using TahaMarket.Infrastructure.Data;

public class SearchService
{
    private readonly ApplicationDbContext _context;
    private readonly FileUrlService _fileUrl;

    public SearchService(ApplicationDbContext context, FileUrlService fileUrl)
    {
        _context = context;
        _fileUrl = fileUrl;
    }

    // =========================
    // SEARCH MAIN ENTRY
    // =========================
    public async Task<object> Search(SearchRequest request)
    {
        return request.Type switch
        {
            SearchType.Store => await SearchStores(request),
            SearchType.Product => await SearchProducts(request),
            _ => throw new Exception("Invalid search type")
        };
    }

    // =========================
    // SEARCH STORES
    // =========================
    private async Task<object> SearchStores(SearchRequest request)
    {
        var query = _context.Stores
            .AsNoTracking()
            .Where(s =>
                s.Name.Contains(request.Query) ||
                s.Description.Contains(request.Query));

        var total = await query.CountAsync();

        var data = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Description,
                ImageUrl = _fileUrl.GetFullUrl(s.ImageUrl)
            })
            .ToListAsync();

        return new
        {
            Total = total,
            Data = data
        };
    }

    // =========================
    // SEARCH PRODUCTS
    // =========================
    private async Task<object> SearchProducts(SearchRequest request)
    {
        var query = _context.Products
            .AsNoTracking()
            .Where(p =>
                p.Name.Contains(request.Query) ||
                p.Description.Contains(request.Query));

        var total = await query.CountAsync();

        var data = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                ImageUrl = _fileUrl.GetFullUrl(p.ImageUrl),

                Price = p.Variants.Any()
                    ? p.Variants.Min(v => v.Price)
                    : 0,

                Store = new
                {
                    p.Store.Id,
                    p.Store.Name
                }
            })
            .ToListAsync();

        return new
        {
            Total = total,
            Data = data
        };
    }
}