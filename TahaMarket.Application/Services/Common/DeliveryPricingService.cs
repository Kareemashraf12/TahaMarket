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
        private readonly DistanceService _distance;

        private const string CACHE_KEY = "delivery_pricing_latest";

        public DeliveryPricingService(
            ApplicationDbContext context,
            IMemoryCache cache,
            DistanceService distance)
        {
            _context = context;
            _cache = cache;
            _distance = distance;
        }

        // =========================
        // UPDATE PRICING (ADMIN)
        // =========================
        public async Task<DeliveryPricingResponse> UpdatePricing(UpdateDeliveryPricingRequest request)
        {
            // =========================
            // VALIDATION
            // =========================
            if (request.BaseFee < 0)
                throw new Exception("Base fee cannot be negative");

            if (request.PricePerKm <= 0)
                throw new Exception("Price per km must be greater than 0");

            if (request.MinFee < 0 || request.MaxFee <= 0)
                throw new Exception("Invalid min/max fee");

            if (request.MinFee > request.MaxFee)
                throw new Exception("Min fee cannot be greater than max fee");

            // =========================
            // SAVE NEW VERSION
            // =========================
            var pricing = new DeliveryPricing
            {
                BaseFee = request.BaseFee,
                PricePerKm = request.PricePerKm,
                MinFee = request.MinFee,
                MaxFee = request.MaxFee,
                CreatedAt = DateTime.UtcNow
            };

            _context.DeliveryPricings.Add(pricing);
            await _context.SaveChangesAsync();

            // =========================
            // CLEAR CACHE
            // =========================
            _cache.Remove(CACHE_KEY);

            return new DeliveryPricingResponse
            {
                BaseFee = pricing.BaseFee,
                PricePerKm = pricing.PricePerKm,
                MinFee = pricing.MinFee,
                MaxFee = pricing.MaxFee,
                CreatedAt = pricing.CreatedAt
            };
        }

        // =========================
        // GET LATEST PRICING (CACHED)
        // =========================
        private async Task<DeliveryPricing> GetLatestPricing()
        {
            if (_cache.TryGetValue(CACHE_KEY, out DeliveryPricing? cached) && cached != null)
                return cached;

            var pricing = await _context.DeliveryPricings
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (pricing == null)
                throw new Exception("Delivery pricing is not configured. Please setup pricing first.");

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(3))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

            _cache.Set(CACHE_KEY, pricing, cacheOptions);

            return pricing;
        }
        // =========================
        // CALCULATE DELIVERY FEE
        // =========================
        public async Task<decimal> CalculateFee(double distanceKm)
        {
            if (distanceKm < 0)
                throw new Exception("Invalid distance");

            if (distanceKm > 42)
                throw new Exception("Distance too large for delivery");

            var pricing = await GetLatestPricing();

            var fee = pricing.BaseFee + ((decimal)distanceKm * pricing.PricePerKm);

            fee = Math.Max(fee, pricing.MinFee);
            fee = Math.Min(fee, pricing.MaxFee);

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



        // =========================
        // CALCULATE DELIVERY COST (MAIN METHOD)
        // =========================
        public async Task<DeliveryCostDto> CalculateDeliveryCost(
       double storeLat,
       double storeLng,
       double userLat,
       double userLng,
       decimal serviceFee = 0)
        {
            if (storeLat == 0 || storeLng == 0 || userLat == 0 || userLng == 0)
                throw new Exception("Invalid coordinates");

            var distanceKm = _distance.CalculateDistanceKm(
                storeLat,
                storeLng,
                userLat,
                userLng
            );

            distanceKm = Math.Round(distanceKm, 2);

            var pricing = await GetLatestPricing();

            var deliveryFee = pricing.BaseFee + ((decimal)distanceKm * pricing.PricePerKm);

            deliveryFee = Math.Max(deliveryFee, pricing.MinFee);
            deliveryFee = Math.Min(deliveryFee, pricing.MaxFee);

            deliveryFee = Math.Round(deliveryFee, 2);

            var totalDeliveryCost = deliveryFee + serviceFee;

            return new DeliveryCostDto
            {
                DistanceKm = distanceKm,
                BaseDeliveryFee = deliveryFee,
                ServiceFee = serviceFee,
                TotalDeliveryCost = totalDeliveryCost

            };
        }
    }
}