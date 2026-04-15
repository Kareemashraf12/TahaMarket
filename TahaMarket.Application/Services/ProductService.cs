using Microsoft.EntityFrameworkCore;
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
        // =========================
        // VALIDATE CATEGORY
        // =========================
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.StoreId == storeId);

        if (category == null)
            throw new Exception("Category not found");

        // =========================
        // PARSE VARIANTS
        // =========================
        List<CreateVariantRequest> variants;

        try
        {
            variants = System.Text.Json.JsonSerializer.Deserialize<List<CreateVariantRequest>>(
                request.Variants,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
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

        // =========================
        // SAVE IMAGE
        // =========================
        var imagePath = await _imageService.SaveImage(request.Image, "images/products");

        // =========================
        // CREATE PRODUCT
        // =========================
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
                Price = v.Price
            }).ToList()
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var minPrice = product.Variants.Min(v => v.Price);

        // =========================
        // RESPONSE
        // =========================
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            ImageUrl = _fileUrl.GetFullUrl(product.ImageUrl),
            CategoryId = product.CategoryId,
            CategoryName = category.Name,
            OldPrice = minPrice,
            FinalPrice = minPrice,
            HasDiscount = false,
            DiscountPercentage = 0,
            Variants = product.Variants.Select(v => new ProductVariantResponse
            {
                Id = v.Id,
                Size = v.Name,
                Price = v.Price
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
                    .Count(r => r.TargetType == RatingTargetType.Product && r.TargetId == p.Id)
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
            RatingsCount = p.CountRating
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

                Variants = p.Variants.Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    Price = v.Price
                }).ToList(),

                MinPrice = p.Variants.Any()
                    ? p.Variants.Min(v => v.Price)
                    : 0,

                MaxPrice = p.Variants.Any()
                    ? p.Variants.Max(v => v.Price)
                    : 0,

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
            .FirstOrDefaultAsync(p => p.Id == productId && p.StoreId == storeId);

        if (product == null)
            throw new Exception("Product not found");

        // =========================
        // UPDATE BASIC DATA
        // =========================
        if (!string.IsNullOrEmpty(request.Name))
            product.Name = request.Name;

        if (request.Description != null)
            product.Description = request.Description;

        // =========================
        // UPDATE IMAGE
        // =========================
        if (request.Image != null)
        {
            product.ImageUrl = await _imageService.SaveImage(
                request.Image,
                "images/products"
            );
        }

        // =========================
        // UPDATE VARIANTS
        // =========================
        if (!string.IsNullOrEmpty(request.Variants))
        {
            List<CreateVariantRequest> variants;

            try
            {
                variants = System.Text.Json.JsonSerializer.Deserialize<List<CreateVariantRequest>>(
                    request.Variants,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                ) ?? new List<CreateVariantRequest>();
            }
            catch
            {
                throw new Exception("Invalid variants format");
            }

            if (!variants.Any())
                throw new Exception("Product must have at least one variant");

            if (variants.Any(v => v.Price <= 0))
                throw new Exception("Invalid variant price");

           
            _context.ProductVariants.RemoveRange(product.Variants);

            
            product.Variants = variants.Select(v => new ProductVariant
            {
                Name = v.Size,
                Price = v.Price
            }).ToList();
        }

        await _context.SaveChangesAsync();
    }

    public async Task Delete(Guid productId, Guid storeId)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId && p.StoreId == storeId);

        if (product == null)
            throw new Exception("Product not found");

        // delete ratings
        var ratings = _context.Ratings.Where(r =>
            r.TargetType == RatingTargetType.Product &&
            r.TargetId == productId);

        _context.Ratings.RemoveRange(ratings);

        // delete product
        _context.Products.Remove(product);

        await _context.SaveChangesAsync();
    }
}