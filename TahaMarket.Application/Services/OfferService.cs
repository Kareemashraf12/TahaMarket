using Microsoft.EntityFrameworkCore;
using TahaMarket.Application.DTOs;
using TahaMarket.Domain.Entities;
using TahaMarket.Domain.Enums;
using TahaMarket.Infrastructure.Data;

public class OfferService
{
    private readonly ApplicationDbContext _context;

    public OfferService(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================
    // CREATE OFFER
    // =========================
    public async Task<OfferDto> Create(CreateOfferRequest request)
    {
        var now = DateTime.UtcNow;

        // -------------------------
        // Validate Target
        // -------------------------
        if (request.TargetType == OfferTargetType.Product)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == request.TargetId);

            if (product == null)
                throw new Exception("Product not found");
        }

        if (request.TargetType == OfferTargetType.Category)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == request.TargetId);

            if (!categoryExists)
                throw new Exception("Category not found");
        }

        // -------------------------
        // Overlap check (important)
        // -------------------------
        var hasConflict = await _context.Offers.AnyAsync(o =>
            o.TargetId == request.TargetId &&
            o.TargetType == request.TargetType &&
            o.IsActive &&
            o.StartDate <= request.EndDate &&
            o.EndDate >= request.StartDate
        );

        if (hasConflict)
            throw new Exception("Conflicting offer already exists");

        var offer = new Offer
        {
            Title = request.Title,
            DiscountPercentage = request.DiscountPercentage,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TargetId = request.TargetId,
            TargetType = request.TargetType,
            IsActive = true,
            CreatedAt = now
        };

        _context.Offers.Add(offer);
        await _context.SaveChangesAsync();

        return Map(offer);
    }

    // =========================
    // GET ACTIVE OFFERS
    // =========================
    public async Task<List<OfferDto>> GetActiveOffers()
    {
        var now = DateTime.UtcNow;

        var offers = await _context.Offers
            .AsNoTracking()
            .Where(o =>
                o.StartDate <= now &&
                o.EndDate >= now &&
                o.IsActive)
            .ToListAsync();

        return offers.Select(Map).ToList();
    }

    // =========================
    // GET BY ID
    // =========================
    public async Task<OfferDto> GetById(Guid id)
    {
        var offer = await _context.Offers
            .FirstOrDefaultAsync(o => o.Id == id);

        if (offer == null)
            throw new Exception("Offer not found");

        return Map(offer);
    }

    // =========================
    // UPDATE OFFER
    // =========================
    public async Task<OfferDto> Update(Guid id, CreateOfferRequest request)
    {
        var offer = await _context.Offers
            .FirstOrDefaultAsync(o => o.Id == id);

        if (offer == null)
            throw new Exception("Offer not found");

        offer.Title = request.Title;
        offer.DiscountPercentage = request.DiscountPercentage;
        offer.StartDate = request.StartDate;
        offer.EndDate = request.EndDate;
        offer.TargetId = request.TargetId;
        offer.TargetType = request.TargetType;

        await _context.SaveChangesAsync();

        return Map(offer);
    }

    // =========================
    // DELETE OFFER
    // =========================
    public async Task Delete(Guid id)
    {
        var offer = await _context.Offers
            .FirstOrDefaultAsync(o => o.Id == id);

        if (offer == null)
            throw new Exception("Offer not found");

        _context.Offers.Remove(offer);
        await _context.SaveChangesAsync();
    }

    // =========================
    // AUTO EXPIRE (IMPORTANT)
    // =========================
    public async Task DeactivateExpiredOffers()
    {
        var now = DateTime.UtcNow;

        var expiredOffers = await _context.Offers
            .Where(o => o.IsActive && o.EndDate < now)
            .ToListAsync();

        foreach (var offer in expiredOffers)
        {
            offer.IsActive = false;
        }

        await _context.SaveChangesAsync();
    }

    // =========================
    // MAP
    // =========================
    private OfferDto Map(Offer o)
    {
        return new OfferDto
        {
            Id = o.Id,
            Title = o.Title,
            DiscountPercentage = o.DiscountPercentage,
            StartDate = o.StartDate,
            EndDate = o.EndDate,
            IsActive = o.IsActive,
            TargetId = o.TargetId,
            TargetType = o.TargetType
        };
    }
}