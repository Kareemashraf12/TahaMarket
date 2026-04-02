using Microsoft.EntityFrameworkCore;
using TahaMarket.Domain.Entities;
using TahaMarket.Infrastructure.Data;

public class RatingService
{
    private readonly ApplicationDbContext _context;

    public RatingService(ApplicationDbContext context)
    {
        _context = context;
    }

    // 🔥 Rate Store
    public async Task RateStore(Guid storeId, Guid userId, CreateRatingRequest request)
    {
        var exists = await _context.StoreRatings
            .FirstOrDefaultAsync(r => r.StoreId == storeId && r.UserId == userId);

        if (exists != null)
        {
            exists.Value = request.Value;
            exists.Comment = request.Comment;
        }
        else
        {
            var rating = new StoreRating
            {
                StoreId = storeId,
                UserId = userId,
                Value = request.Value,
                Comment = request.Comment
            };

            _context.StoreRatings.Add(rating);
        }

        await _context.SaveChangesAsync();
    }

    // 🔥 Rate Product
    public async Task RateProduct(Guid productId, Guid userId, CreateRatingRequest request)
    {
        var exists = await _context.ProductRatings
            .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);

        if (exists != null)
        {
            exists.Value = request.Value;
            exists.Comment = request.Comment;
        }
        else
        {
            var rating = new ProductRating
            {
                ProductId = productId,
                UserId = userId,
                Value = request.Value,
                Comment = request.Comment
            };

            _context.ProductRatings.Add(rating);
        }

        await _context.SaveChangesAsync();
    }

    // 🔥 Get Store Rating
    public async Task<object> GetStoreRating(Guid storeId)
    {
        var ratings = await _context.StoreRatings
            .Where(r => r.StoreId == storeId)
            .ToListAsync();

        return new
        {
            Average = ratings.Any() ? ratings.Average(r => r.Value) : 0,
            Count = ratings.Count
        };
    }

    //  Get Product Rating
    public async Task<object> GetProductRating(Guid productId)
    {
        var ratings = await _context.ProductRatings
            .Where(r => r.ProductId == productId)
            .ToListAsync();

        return new
        {
            Average = ratings.Any() ? ratings.Average(r => r.Value) : 0,
            Count = ratings.Count
        };
    }
}