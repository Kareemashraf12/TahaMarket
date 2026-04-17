using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TahaMarket.Application.DTOs;
using TahaMarket.Domain.Entities;
using TahaMarket.Infrastructure.Data;

namespace TahaMarket.Application.Services.Common
{
    public class DeliveryPricingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        private const string CACHE_KEY = "delivery_pricing_latest";

        public DeliveryPricingService(
            ApplicationDbContext context,
            IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // =========================
        // SET PRICING (ADMIN)
        // =========================
        public async Task SetPricing(decimal baseFee, decimal pricePerKm, decimal minFee, decimal maxFee)
        {
            // VALIDATION
            if (baseFee < 0)
                throw new Exception("Base fee cannot be negative");

            if (pricePerKm <= 0)
                throw new Exception("Price per km must be greater than 0");

            if (minFee < 0 || maxFee <= 0)
                throw new Exception("Invalid min/max fee");

            if (minFee > maxFee)
                throw new Exception("Min fee cannot be greater than max fee");

            // CREATE NEW VERSION ( save History )
            var pricing = new DeliveryPricing
            {
                BaseFee = baseFee,
                PricePerKm = pricePerKm,
                MinFee = minFee,
                MaxFee = maxFee,
                CreatedAt = DateTime.UtcNow
            };

            _context.DeliveryPricings.Add(pricing);
            await _context.SaveChangesAsync();

            
            _cache.Remove(CACHE_KEY);
        }

        // =========================
        // GET LATEST PRICING (CACHED)
        // =========================
        private async Task<DeliveryPricing> GetLatestPricing()
        {
            // CHECK CACHE
            if (_cache.TryGetValue(CACHE_KEY, out DeliveryPricing cachedPricing))
                return cachedPricing;

            // GET FROM DB
            var pricing = await _context.DeliveryPricings
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (pricing == null)
                throw new Exception("Delivery pricing is not configured");

            // SAVE IN CACHE (5 minutes)
            _cache.Set(CACHE_KEY, pricing, TimeSpan.FromMinutes(5));

            return pricing;
        }

        // =========================
        // CALCULATE DELIVERY FEE
        // =========================
        public async Task<decimal> CalculateFee(double distanceKm)
        {
            var pricing = await GetLatestPricing();

            var fee = pricing.BaseFee + ((decimal)distanceKm * pricing.PricePerKm);

            // APPLY MIN
            if (fee < pricing.MinFee)
                fee = pricing.MinFee;

            // APPLY MAX
            if (fee > pricing.MaxFee)
                fee = pricing.MaxFee;

            return Math.Round(fee, 2);
        }

        // =========================
        // GET CURRENT PRICING
        // =========================
        public async Task<DeliveryPricingResponse> GetCurrentPricing()
        {
            var pricing = await GetLatestPricing();

            return new DeliveryPricingResponse
            {
                BaseFee = pricing.BaseFee,
                PricePerKm = pricing.PricePerKm,
                MinFee = pricing.MinFee,
                MaxFee = pricing.MaxFee,
                CreatedAt = pricing.CreatedAt
            };
        }
    }
}