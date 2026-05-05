using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services.Common;
using TahaMarket.Domain.Entities;
using TahaMarket.Domain.Enums;
using TahaMarket.Infrastructure.Data;

public class ProductService
{
    private readonly ApplicationDbContext _context;
    private readonly ImageService _imageService;
    private readonly FileUrlService _fileUrl;

    public ProductService(
        ApplicationDbContext context,
        ImageService imageService,
        FileUrlService fileUrl)
    {
        _context = context;
        _imageService = imageService;
        _fileUrl = fileUrl;
    }

    // =========================
    // CREATE PRODUCT
    // =========================
    public async Task<ProductResponse> Create(Guid storeId, CreateProductRequest request)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.StoreId == storeId);

        if (category == null)
            throw new Exception("Category not found");

        List<CreateVariantRequest> variants;

        try
        {
            variants = JsonSerializer.Deserialize<List<CreateVariantRequest>>(
                request.Variants,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<CreateVariantRequest>();
        }
        catch
        {
            throw new Exception("Invalid variants format");
        }

        if (!variants.Any())
            throw new Exception("Product must have at least one variant");

        if (variants.Any(v => v.Price <= 0))
            throw new Exception("Variant price must be greater than 0");

        // STOCK VALIDATION
        foreach (var v in variants)
        {
            if (v.IsStockTracked && (v.StockQuantity == null || v.StockQuantity < 0))
                throw new Exception($"Variant {v.Size} must have valid stock");

            if (!v.IsStockTracked && v.StockQuantity != null)
                throw new Exception($"Variant {v.Size} should not have stock");
        }

        var imagePath = await _imageService.SaveImage(request.Image, "images/products");

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            ImageUrl = imagePath,
            CategoryId = request.CategoryId,
            StoreId = storeId,

            Variants = variants.Select(v => new ProductVariant
            {
                Name = v.Size,
                Price = v.Price,
                IsStockTracked = v.IsStockTracked,
                StockQuantity = v.IsStockTracked ? v.StockQuantity : null
            }).ToList()
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var minPrice = product.Variants.Min(v => v.Price);

        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            ImageUrl = _fileUrl.GetFullUrl(product.ImageUrl),
            CategoryId = product.CategoryId,
            CategoryName = category.Name,

            OldPrice = minPrice,
            FinalPrice = minPrice,

            Variants = product.Variants.Select(v => new ProductVariantResponse
            {
                Id = v.Id,
                Size = v.Name,
                Price = v.Price,
                IsStockTracked = v.IsStockTracked,
                StockQuantity = v.StockQuantity
            }).ToList()
        };
    }
    // =========================
    // GET ALL (HOME OPTIMIZED)
    // =========================
    public async Task<PaginatedResponse<ProductListDto>> GetAllProducts(PaginationRequest request)
    {
        var now = DateTime.UtcNow;

        var query = _context.Products
            .AsNoTracking()
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.ImageUrl,
                p.CategoryId,

                // =========================
                // PRICE
                // =========================
                MinPrice = p.Variants.Any()
                    ? p.Variants.Min(v => v.Price)
                    : 0,

                // =========================
                // DISCOUNT
                // =========================
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

                // =========================
                // RATING
                // =========================
                AvgRating = _context.Ratings
                    .Where(r => r.TargetType == RatingTargetType.Product && r.TargetId == p.Id)
                    .Average(r => (double?)r.Value) ?? 0,

                CountRating = _context.Ratings
                    .Count(r => r.TargetType == RatingTargetType.Product && r.TargetId == p.Id),

                // =========================
                // VARIANTS (IMPORTANT CHANGE)
                // =========================
                Variants = p.Variants.Select(v => new
                {
                    v.Id,
                    v.Name,
                    v.Price,
                    v.IsStockTracked,
                    v.StockQuantity
                }).ToList()
            });

        var totalCount = await query.CountAsync();

        var data = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var result = data.Select(p => new ProductListDto
        {
            Id = p.Id,
            Name = p.Name,
            ImageUrl = _fileUrl.GetFullUrl(p.ImageUrl),

            OldPrice = p.MinPrice,
            DiscountPercentage = p.Discount,
            HasDiscount = p.Discount > 0,

            FinalPrice = p.Discount > 0
                ? p.MinPrice - (p.MinPrice * p.Discount / 100)
                : p.MinPrice,

            AverageRating = p.AvgRating,
            RatingsCount = p.CountRating,

            // =========================
            // VARIANTS WITH STOCK
            // =========================
            Variants = p.Variants.Select(v => new ProductVariantResponse
            {
                Id = v.Id,
                Size = v.Name,
                Price = v.Price,
                IsStockTracked = v.IsStockTracked,
                StockQuantity = v.StockQuantity
            }).ToList()
        }).ToList();

        return new PaginatedResponse<ProductListDto>
        {
            Page = request.Page,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
            Data = result
        };
    }

    // =========================
    // GET DETAILS
    // =========================
    public async Task<ProductDetailsDto> GetDetails(Guid productId)
    {
        var now = DateTime.UtcNow;

        var product = await _context.Products
            .Where(p => p.Id == productId)
            .Select(p => new ProductDetailsDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ImageUrl = _fileUrl.GetFullUrl(p.ImageUrl),

                StoreId = p.StoreId,
                StoreName = p.Store.Name,

                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,

                // =========================
                // VARIANTS
                // =========================
                Variants = p.Variants.Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    IsStockTracked = v.IsStockTracked
                }).ToList(),

                // =========================
                // ADD-ONS
                // =========================
                AddOnGroups = _context.AddOnGroups
                    .Where(g => g.ProductId == p.Id && g.IsActive)
                    .Select(g => new AddOnGroupDto
                    {
                        Id = g.Id,
                        Name = g.Name,

                        Options = g.Options
                            .Where(o => o.IsActive)
                            .Select(o => new AddOnOptionDto
                            {
                                Id = o.Id,
                                Name = o.Name,
                                Price = o.Price
                            }).ToList()
                    }).ToList(),

                // =========================
                // PRICES
                // =========================
                MinPrice = p.Variants.Any()
                    ? p.Variants.Min(v => v.Price)
                    : 0,

                MaxPrice = p.Variants.Any()
                    ? p.Variants.Max(v => v.Price)
                    : 0,

                // =========================
                //  DISCOUNT (IMPORTANT)
                // =========================
                DiscountPercentage = _context.Offers
                    .Where(o =>
                        o.IsActive &&
                        o.StartDate <= now &&
                        o.EndDate >= now &&
                        (
                            (o.TargetType == OfferTargetType.Product && o.TargetId == p.Id) ||
                            (o.TargetType == OfferTargetType.Category && o.TargetId == p.CategoryId)
                        ))
                    .Select(o => (decimal?)o.DiscountPercentage)
                    .Max() ?? 0,

                OldPrice = p.Variants.Any()
                    ? p.Variants.Min(v => v.Price)
                    : 0,

        
                FinalPrice = p.Variants.Any()
                    ? p.Variants.Min(v => v.Price) -
                      (p.Variants.Min(v => v.Price) *
                       (
                           _context.Offers
                               .Where(o =>
                                   o.IsActive &&
                                   o.StartDate <= now &&
                                   o.EndDate >= now &&
                                   (
                                       (o.TargetType == OfferTargetType.Product && o.TargetId == p.Id) ||
                                       (o.TargetType == OfferTargetType.Category && o.TargetId == p.CategoryId)
                                   ))
                               .Select(o => (decimal?)o.DiscountPercentage)
                               .Max() ?? 0
                       ) / 100)
                    : 0,

                // =========================
                // RATING
                // =========================
                Rating = new RatingSummaryDto
                {
                    Count = _context.Ratings.Count(r =>
                        r.TargetType == RatingTargetType.Product &&
                        r.TargetId == p.Id),

                    Average = _context.Ratings
                        .Where(r => r.TargetType == RatingTargetType.Product &&
                                    r.TargetId == p.Id)
                        .Average(r => (double?)r.Value) ?? 0,

                    Star1 = _context.Ratings.Count(r =>
                        r.TargetType == RatingTargetType.Product &&
                        r.TargetId == p.Id && r.Value == 1),

                    Star2 = _context.Ratings.Count(r =>
                        r.TargetType == RatingTargetType.Product &&
                        r.TargetId == p.Id && r.Value == 2),

                    Star3 = _context.Ratings.Count(r =>
                        r.TargetType == RatingTargetType.Product &&
                        r.TargetId == p.Id && r.Value == 3),

                    Star4 = _context.Ratings.Count(r =>
                        r.TargetType == RatingTargetType.Product &&
                        r.TargetId == p.Id && r.Value == 4),

                    Star5 = _context.Ratings.Count(r =>
                        r.TargetType == RatingTargetType.Product &&
                        r.TargetId == p.Id && r.Value == 5)
                }
            })
            .FirstOrDefaultAsync();

        if (product == null)
            throw new Exception("Product not found");

        return product;
    }
    // ===========================
    // Get By Category 
    // ===========================

    public async Task<PaginatedResponse<ProductListDto>> GetByCategory(
    Guid categoryId,
    PaginationRequest request)
    {
        var query = _context.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == categoryId)
            .Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = _fileUrl.GetFullUrl(p.ImageUrl),

                // price from variants
                OldPrice = p.Variants.Any()
                    ? p.Variants.Min(v => v.Price)
                    : 0,

                FinalPrice = p.Variants.Any()
                    ? p.Variants.Min(v => v.Price)
                    : 0,

                HasDiscount = false,
                DiscountPercentage = 0,

                // rating (generic system)
                AverageRating = _context.Ratings
                    .Where(r => r.TargetType == RatingTargetType.Product
                             && r.TargetId == p.Id)
                    .Average(r => (double?)r.Value) ?? 0,

                RatingsCount = _context.Ratings
                    .Count(r => r.TargetType == RatingTargetType.Product
                             && r.TargetId == p.Id)
            });

        var totalCount = await query.CountAsync();

        var data = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PaginatedResponse<ProductListDto>
        {
            Page = request.Page,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
            Data = data
        };
    }

    // =======================
    // Update 
    // =======================
    public async Task Update(Guid productId, Guid storeId, UpdateProductRequest request)
    {
        var product = await _context.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p =>
                p.Id == productId &&
                p.StoreId == storeId);

        if (product == null)
            throw new Exception("Product not found or not allowed");

        // =========================
        // BASIC DATA UPDATE
        // =========================
        if (!string.IsNullOrWhiteSpace(request.Name))
            product.Name = request.Name;

        if (request.Description != null)
            product.Description = request.Description;

        // =========================
        // IMAGE UPDATE
        // =========================
        if (request.Image != null)
        {
            product.ImageUrl = await _imageService.SaveImage(
                request.Image,
                "images/products"
            );
        }

        // =========================
        // VARIANTS UPDATE (NO ID LOSS)
        // =========================
        if (!string.IsNullOrWhiteSpace(request.Variants))
        {
            var variants = JsonSerializer.Deserialize<List<CreateVariantRequest>>(
                request.Variants,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? new List<CreateVariantRequest>();

            if (!variants.Any())
                throw new Exception("At least one variant required");

            if (variants.Any(v => v.Price <= 0))
                throw new Exception("Invalid variant price");

            // =========================
            // UPDATE EXISTING VARIANTS
            // =========================
            foreach (var existing in product.Variants.ToList())
            {
                var updated = variants.FirstOrDefault(v => v.Id == existing.Id);

                if (updated != null)
                {
                    existing.Name = updated.Size;
                    existing.Price = updated.Price;

                    // STOCK UPDATE
                    existing.IsStockTracked = updated.IsStockTracked;
                    existing.StockQuantity = updated.IsStockTracked
                        ? updated.StockQuantity
                        : null;
                }
            }

            // =========================
            // ADD NEW VARIANTS
            // =========================
            var newVariants = variants
                .Where(v => v.Id == Guid.Empty)
                .Select(v => new ProductVariant
                {
                    Name = v.Size,
                    Price = v.Price,
                    IsStockTracked = v.IsStockTracked,
                    StockQuantity = v.IsStockTracked ? v.StockQuantity : null
                });

            foreach (var v in newVariants)
            {
                product.Variants.Add(v);
            }

            // =========================
            // DELETE REMOVED VARIANTS
            // =========================
            var incomingIds = variants
                .Where(v => v.Id != Guid.Empty)
                .Select(v => v.Id)
                .ToList();

            var toRemove = product.Variants
                .Where(v => !incomingIds.Contains(v.Id))
                .ToList();

            _context.ProductVariants.RemoveRange(toRemove);
        }

        await _context.SaveChangesAsync();
    }

    // =======================
    // DELETE
    // =======================
    public async Task Delete(Guid productId, Guid? storeId, string role)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
            throw new Exception("Product not found");

        // =========================
        // AUTHORIZATION
        // =========================
        if (string.Equals(role, "store", StringComparison.OrdinalIgnoreCase))
        {
            if (storeId == null)
                throw new Exception("StoreId missing");

            if (product.StoreId != storeId.Value)
                throw new Exception("Not allowed to delete this product");
        }

        // =========================
        // DELETE RELATED DATA
        // =========================

        // Ratings
        var ratings = _context.Ratings.Where(r =>
            r.TargetType == RatingTargetType.Product &&
            r.TargetId == productId);

        _context.Ratings.RemoveRange(ratings);

        // Cart Items 
        var cartItems = _context.CartItems
            .Where(c => c.ProductId == productId);

        var cartItemIds = cartItems.Select(c => c.Id);

        // CartItem AddOns
        var cartAddOns = _context.Set<CartItemAddOn>()
            .Where(a => cartItemIds.Contains(a.CartItemId));

        _context.RemoveRange(cartAddOns);
        _context.RemoveRange(cartItems);

        // Variants
        var variants = _context.ProductVariants
            .Where(v => v.ProductId == productId);

        _context.ProductVariants.RemoveRange(variants);

        // AddOnGroups + Options
        var groups = _context.AddOnGroups
            .Where(g => g.ProductId == productId);

        var groupIds = groups.Select(g => g.Id);

        var options = _context.AddOnOptions
            .Where(o => groupIds.Contains(o.AddOnGroupId));

        _context.AddOnOptions.RemoveRange(options);
        _context.AddOnGroups.RemoveRange(groups);

        
        var orderItems = _context.OrderItems
            .Where(o => o.ProductId == productId);

        _context.OrderItems.RemoveRange(orderItems);

        // =========================
        // DELETE PRODUCT
        // =========================
        _context.Products.Remove(product);

        await _context.SaveChangesAsync();
    }
    // ===========================
    // Get Hot Offers (>= 50% discount)
    // ===========================
    public async Task<List<SpecialProductDto>> GetHotOffers()
    {
        var now = DateTime.UtcNow;

        var activeOffers = _context.Offers
            .Where(o =>
                o.IsActive &&
                o.StartDate <= now &&
                o.EndDate >= now);

        var ratings = _context.Ratings
            .Where(r => r.TargetType == RatingTargetType.Product);

        var products = await _context.Products
            .Where(p => p.Variants.Any())
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.ImageUrl,
                p.StoreId,
                p.CategoryId,

                MinPrice = p.Variants.Min(v => v.Price),

                Discount = activeOffers
                    .Where(o =>
                        (o.TargetType == OfferTargetType.Product && o.TargetId == p.Id) ||
                        (o.TargetType == OfferTargetType.Category && o.TargetId == p.CategoryId))
                    .Select(o => (decimal?)o.DiscountPercentage)
                    .Max() ?? 0,

                AvgRating = ratings
                    .Where(r => r.TargetId == p.Id)
                    .Average(r => (double?)r.Value) ?? 0
            })
            .Where(p => p.Discount >= 50)
            .ToListAsync();

        return products.Select(p => new SpecialProductDto
        {
            Label = "عروض ما تتفوتش",
            ProductId = p.Id,
            ProductName = p.Name,
            ImageUrl = _fileUrl.GetFullUrl(p.ImageUrl),
            StoreId = p.StoreId,
            CategoryId = p.CategoryId,

            AverageRating = p.AvgRating,

            OldPrice = p.MinPrice,

            FinalPrice = p.MinPrice - (p.MinPrice * p.Discount / 100),

            HasDiscount = p.Discount > 0,
            DiscountPercentage = p.Discount
        }).ToList();
    }
    // ===========================
    // Get Top Selling Products
    // ===========================
    public async Task<List<SpecialProductDto>> GetTopSellingProducts(int take = 10)
    {
        var now = DateTime.UtcNow;

        var topSelling = _context.OrderItems
            .GroupBy(oi => oi.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Count = g.Count()
            });

        var products = await topSelling
            .OrderByDescending(ts => ts.Count)
            .Take(take)
            .Join(_context.Products.AsNoTracking(),
                ts => ts.ProductId,
                p => p.Id,
                (ts, p) => new
                {
                    p.Id,
                    p.Name,
                    p.ImageUrl,
                    p.StoreId,
                    p.CategoryId,

                    // ✅ SAFE MIN
                    MinPrice = p.Variants
                        .Select(v => (decimal?)v.Price)
                        .Min() ?? 0,

                    Discount = _context.Offers
                        .Where(o =>
                            o.IsActive &&
                            o.StartDate <= now &&
                            o.EndDate >= now &&
                            (
                                (o.TargetType == OfferTargetType.Product && o.TargetId == p.Id) ||
                                (o.TargetType == OfferTargetType.Category && o.TargetId == p.CategoryId)
                            ))
                        .Select(o => (decimal?)o.DiscountPercentage)
                        .Max() ?? 0,

                    AvgRating = _context.Ratings
                        .Where(r => r.TargetType == RatingTargetType.Product && r.TargetId == p.Id)
                        .Select(r => (double?)r.Value)
                        .Average() ?? 0
                })
            .ToListAsync();

        return products.Select(p => new SpecialProductDto
        {
            Label = "الأكثر مبيعًا",
            ProductId = p.Id,
            ProductName = p.Name,
            ImageUrl = _fileUrl.GetFullUrl(p.ImageUrl),
            StoreId = p.StoreId,
            CategoryId = p.CategoryId,

            AverageRating = p.AvgRating,

            OldPrice = p.MinPrice,

            FinalPrice = p.Discount > 0
                ? p.MinPrice - (p.MinPrice * p.Discount / 100)
                : p.MinPrice,

            HasDiscount = p.Discount > 0,
            DiscountPercentage = p.Discount
        }).ToList();
    }
    // ===========================
    // Get Daily Offers (Active Offers)
    // ===========================
    public async Task<PaginatedResponse<ProductListDto>> GetDailyOffers(PaginationRequest request)
    {
        var now = DateTime.UtcNow;

        var query = _context.Products
            .AsNoTracking()
            .Where(p =>
                _context.Offers.Any(o =>
                    o.IsActive &&
                    o.StartDate <= now &&
                    o.EndDate >= now &&
                    (
                        (o.TargetType == OfferTargetType.Product && o.TargetId == p.Id) ||
                        (o.TargetType == OfferTargetType.Category && o.TargetId == p.CategoryId)
                    )
                )
            )
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.ImageUrl,

                MinPrice = p.Variants.Min(v => v.Price),

                Discount = _context.Offers
                    .Where(o =>
                        o.IsActive &&
                        o.StartDate <= now &&
                        o.EndDate >= now &&
                        (
                            (o.TargetType == OfferTargetType.Product && o.TargetId == p.Id) ||
                            (o.TargetType == OfferTargetType.Category && o.TargetId == p.CategoryId)
                        ))
                    .Select(o => (decimal?)o.DiscountPercentage)
                    .Max() ?? 0,

                AverageRating = _context.Ratings
                    .Where(r =>
                        r.TargetType == RatingTargetType.Product &&
                        r.TargetId == p.Id)
                    .Select(r => (double?)r.Value)
                    .Average() ?? 0,

                RatingsCount = _context.Ratings
                    .Count(r =>
                        r.TargetType == RatingTargetType.Product &&
                        r.TargetId == p.Id)
            });

        // =========================
        // COUNT
        // =========================
        var totalCount = await query.CountAsync();

        // =========================
        // PAGINATION
        // =========================
        var data = await query
            .OrderByDescending(p => p.Discount)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        // =========================
        // MAP
        // =========================
        var result = data.Select(p => new ProductListDto
        {
            Id = p.Id,
            Name = p.Name,
            ImageUrl = _fileUrl.GetFullUrl(p.ImageUrl),

            OldPrice = p.MinPrice,

            DiscountPercentage = p.Discount,
            HasDiscount = p.Discount > 0,

            FinalPrice = p.Discount > 0
                ? p.MinPrice - (p.MinPrice * p.Discount / 100)
                : p.MinPrice,

            AverageRating = Math.Round(p.AverageRating, 1),
            RatingsCount = p.RatingsCount
        }).ToList();

        return new PaginatedResponse<ProductListDto>
        {
            Page = request.Page,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
            Data = result
        };
    }
}