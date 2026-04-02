using Microsoft.EntityFrameworkCore;
using TahaMarket.Application.DTOs;
using TahaMarket.Domain.Entities;
using TahaMarket.Infrastructure.Data;

public class ProductService
{
    private readonly ApplicationDbContext _context;
    private readonly ImageService _imageService;

    public ProductService(ApplicationDbContext context, ImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    //  Create Product
    public async Task<ProductResponse> Create(Guid storeId, CreateProductRequest request)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.StoreId == storeId);

        if (category == null)
            throw new Exception("Invalid Category");

        var imagePath = await _imageService.SaveImage(request.Image, "images/products");

        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            ImageUrl = imagePath,
            CategoryId = request.CategoryId
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            CategoryName = category.Name
        };
    }

    //  Get ALL products for Store 
    public async Task<List<ProductResponse>> GetAllByStore(Guid storeId)
    {
        return await _context.Products
            .Where(p => p.Category.StoreId == storeId)
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name
            })
            .ToListAsync();
    }

    //  Get By Category
    public async Task<List<ProductResponse>> GetByCategory(Guid storeId, Guid categoryId)
    {
        return await _context.Products
            .Where(p => p.CategoryId == categoryId &&
                        p.Category.StoreId == storeId)
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name
            })
            .ToListAsync();
    }

    //  Details
    public async Task<ProductResponse> GetDetails(Guid productId, Guid storeId)
    {
        var product = await _context.Products
            .Where(p => p.Id == productId &&
                        p.Category.StoreId == storeId)
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name
            })
            .FirstOrDefaultAsync();

        if (product == null)
            throw new Exception("Product not found");

        return product;
    }

    //  Update
    public async Task Update(Guid productId, Guid storeId, UpdateProductRequest request)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId &&
                                     p.Category.StoreId == storeId);

        if (product == null)
            throw new Exception("Product not found");

        product.Name = request.Name ?? product.Name;
        product.Price = request.Price != 0 ? request.Price : product.Price;

        if (request.Image != null)
        {
            product.ImageUrl = await _imageService.SaveImage(request.Image, "images/products");
        }

        await _context.SaveChangesAsync();
    }

    //  Delete
    public async Task Delete(Guid productId, Guid storeId)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId &&
                                     p.Category.StoreId == storeId);

        if (product == null)
            throw new Exception("Product not found");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }
}