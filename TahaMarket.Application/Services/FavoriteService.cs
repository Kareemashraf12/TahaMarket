using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TahaMarket.Application.Services.Common;
using TahaMarket.Domain.Entities;
using TahaMarket.Domain.Enums;
using TahaMarket.Infrastructure.Data;

public class FavoriteService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly FileUrlService _fileUrl;

    public FavoriteService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        FileUrlService fileUrl)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _fileUrl = fileUrl;
    }

    // =========================
    // GET USER ID FROM TOKEN
    // =========================
    private Guid GetUserId()
    {
        var userId = _httpContextAccessor.HttpContext?
            .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            throw new Exception("User not authenticated");

        return Guid.Parse(userId);
    }

    // =========================
    // ADD FAVORITE
    // =========================
    public async Task<object> Add(Guid targetId, FavoriteType type)
    {
        var userId = GetUserId();

        bool exists = type switch
        {
            FavoriteType.Product => await _context.Products.AnyAsync(p => p.Id == targetId),
            FavoriteType.Store => await _context.Stores.AnyAsync(s => s.Id == targetId),
            _ => false
        };

        if (!exists)
            throw new Exception("Target not found");

        var already = await _context.Favorites
            .AnyAsync(f => f.UserId == userId && f.TargetId == targetId && f.Type == type);

        if (already)
            throw new Exception("Already in favorites");

        var favorite = new Favorite
        {
            UserId = userId,
            TargetId = targetId,
            Type = type
        };

        _context.Favorites.Add(favorite);
        await _context.SaveChangesAsync();

        return new { Message = "Added to favorites" };
    }

    // =========================
    // REMOVE FAVORITE
    // =========================
    public async Task<object> Remove(Guid targetId, FavoriteType type)
    {
        var userId = GetUserId();

        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f =>
                f.UserId == userId &&
                f.TargetId == targetId &&
                f.Type == type);

        if (favorite == null)
            throw new Exception("Favorite not found");

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync();

        return new { Message = "Removed from favorites" };
    }

    // =========================
    // GET MY FAVORITES
    // =========================
    public async Task<object> GetMyFavorites()
    {
        var userId = GetUserId();
        var now = DateTime.UtcNow;

        var favorites = await _context.Favorites
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .ToListAsync();

        var productIds = favorites
            .Where(f => f.Type == FavoriteType.Product)
            .Select(f => f.TargetId)
            .ToList();

        var storeIds = favorites
            .Where(f => f.Type == FavoriteType.Store)
            .Select(f => f.TargetId)
            .ToList();

        // ================= PRODUCTS =================

        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new
            {
                p.Id,
                p.Name,
                ImageUrl = _fileUrl.GetFullUrl(p.ImageUrl),

                MinPrice = p.Variants.Any()
                    ? p.Variants.Min(v => v.Price)
                    : 0,

                Discount = _context.Offers
                    .Where(o =>
                        o.IsActive &&
                        o.StartDate <= now &&
                        o.EndDate >= now &&
                        (
                            (o.TargetType == OfferTargetType.Product && o.TargetId == p.Id) ||
                            (o.TargetType == OfferTargetType.Category && o.TargetId == p.CategoryId)
                        ))
                    .OrderByDescending(o => o.TargetType == OfferTargetType.Product)
                    .ThenByDescending(o => o.DiscountPercentage)
                    .Select(o => (decimal?)o.DiscountPercentage)
                    .FirstOrDefault() ?? 0,

                AvgRating = _context.Ratings
                    .Where(r => r.TargetType == RatingTargetType.Product && r.TargetId == p.Id)
                    .Average(r => (double?)r.Value) ?? 0,

                CountRating = _context.Ratings
                    .Count(r => r.TargetType == RatingTargetType.Product && r.TargetId == p.Id),

                Type = "Product"
            })
            .ToListAsync();

        var productResult = products.Select(p => new
        {
            p.Id,
            p.Name,
            p.ImageUrl,

            OldPrice = p.MinPrice,
            DiscountPercentage = p.Discount,
            HasDiscount = p.Discount > 0,

            FinalPrice = p.Discount > 0
                ? p.MinPrice - (p.MinPrice * p.Discount / 100)
                : p.MinPrice,

            AverageRating = p.AvgRating,
            RatingsCount = p.CountRating,

            p.Type
        });

        // ================= STORES =================

        var stores = await _context.Stores
            .Where(s => storeIds.Contains(s.Id))
            .Select(s => new
            {
                s.Id,
                s.Name,
                ImageUrl = _fileUrl.GetFullUrl(s.ImageUrl),

                AvgRating = _context.Ratings
                    .Where(r => r.TargetType == RatingTargetType.Store && r.TargetId == s.Id)
                    .Average(r => (double?)r.Value) ?? 0,

                CountRating = _context.Ratings
                    .Count(r => r.TargetType == RatingTargetType.Store && r.TargetId == s.Id),

                Type = "Store"
            })
            .ToListAsync();

        var storeResult = stores.Select(s => new
        {
            s.Id,
            s.Name,
            s.ImageUrl,

            AverageRating = s.AvgRating,
            RatingsCount = s.CountRating,

            s.Type
        });

        // ================= FINAL =================

        return productResult.Concat<object>(storeResult);
    }
}