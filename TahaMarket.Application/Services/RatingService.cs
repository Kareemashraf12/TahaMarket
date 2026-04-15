using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TahaMarket.Application.DTOs;
using TahaMarket.Domain.Entities;
using TahaMarket.Domain.Enums;
using TahaMarket.Infrastructure.Data;

public class RatingService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public RatingService(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    // =========================
    // ADD OR UPDATE RATING
    // =========================
    public async Task AddOrUpdate(Guid userId, AddRatingRequest request)
    {
        var existing = await _context.Ratings
            .FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.TargetId == request.TargetId &&
                r.TargetType == request.TargetType);

        if (existing != null)
        {
            existing.Value = request.Value;
            existing.Comment = request.Comment;
            existing.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.Ratings.Add(new Rating
            {
                UserId = userId,
                TargetId = request.TargetId,
                TargetType = request.TargetType,
                Value = request.Value,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        RemoveCache(request.TargetId, request.TargetType);
    }

    // =========================
    // GET SUMMARY (OPTIMIZED + CACHE)
    // =========================
    public async Task<RatingSummaryDto> GetSummary(Guid targetId, RatingTargetType type)
    {
        var key = GetCacheKey(targetId, type);

        if (_cache.TryGetValue(key, out RatingSummaryDto cached))
            return cached;

        var data = await _context.Ratings
            .Where(r => r.TargetId == targetId && r.TargetType == type)
            .GroupBy(x => 1)
            .Select(g => new RatingSummaryDto
            {
                Count = g.Count(),
                Average = g.Average(x => x.Value),

                Star1 = g.Count(x => x.Value == 1),
                Star2 = g.Count(x => x.Value == 2),
                Star3 = g.Count(x => x.Value == 3),
                Star4 = g.Count(x => x.Value == 4),
                Star5 = g.Count(x => x.Value == 5)
            })
            .FirstOrDefaultAsync();

        var result = data ?? new RatingSummaryDto();

        _cache.Set(key, result, TimeSpan.FromMinutes(10));

        return result;
    }

    // =========================
    // GET COMMENTS (PAGINATION)
    // =========================
    public async Task<PagedResult<RatingCommentDto>> GetComments(
        Guid targetId,
        RatingTargetType type,
        int page,
        int pageSize)
    {
        var query = _context.Ratings
            .AsNoTracking()
            .Where(r => r.TargetId == targetId && r.TargetType == type);

        var totalCount = await query.CountAsync();

        var data = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(r => r.User)
            .Select(r => new RatingCommentDto
            {
                UserName = r.User.Name,
                UserImage = r.User.ImageUrl,
                Value = r.Value,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<RatingCommentDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Data = data
        };
    }

    // =========================
    // OVERLOAD (DEFAULT PAGINATION)
    // =========================
    public Task<PagedResult<RatingCommentDto>> GetComments(
        Guid targetId,
        RatingTargetType type)
    {
        return GetComments(targetId, type, 1, 10);
    }

    // =========================
    // GET FULL DETAILS
    // =========================
    public async Task<RatingDetailsDto> GetFullDetails(Guid targetId, RatingTargetType type)
    {
        var summary = await GetSummary(targetId, type);
        var comments = await GetComments(targetId, type, 1, 10);

        return new RatingDetailsDto
        {
            Summary = summary,
            Comments = comments
        };
    }

    // =========================
    // DELETE RATING
    // =========================
    public async Task Delete(Guid userId, Guid targetId, RatingTargetType type)
    {
        var rating = await _context.Ratings
            .FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.TargetId == targetId &&
                r.TargetType == type);

        if (rating == null)
            throw new Exception("Rating not found");

        _context.Ratings.Remove(rating);
        await _context.SaveChangesAsync();

        RemoveCache(targetId, type);
    }

    // =========================
    // CACHE HELPERS
    // =========================
    private string GetCacheKey(Guid targetId, RatingTargetType type)
    {
        return $"rating_summary_{type}_{targetId}";
    }

    private void RemoveCache(Guid targetId, RatingTargetType type)
    {
        _cache.Remove(GetCacheKey(targetId, type));
    }
}