using Microsoft.EntityFrameworkCore;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services.Common;
using TahaMarket.Domain.Entities;
using TahaMarket.Infrastructure.Data;

public class ProductService
{
    private readonly ApplicationDbContext _context;
    private readonly ImageService _imageService;
    private readonly FileUrlService _fileUrl;

    public ProductService(ApplicationDbContext context, ImageService imageService , FileUrlService fileUrl)
    {
        _context = context;
        _imageService = imageService;
        _fileUrl = fileUrl;
    }

    //  Create Product
    public async Task<ProductResponse> Create(Guid storeId, CreateProductRequest request)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.StoreId == storeId);

        if (category == null)
            throw new Exception("Category not found or does not belong to store");

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
            ImageUrl = _fileUrl.GetFullUrl(product.ImageUrl),
            CategoryId = product.CategoryId,
            CategoryName = category.Name
        };
    }

    public async Task<List<ProductResponse>> GetAllProducts()
    {
        return await _context.Products
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                ImageUrl = _fileUrl.GetFullUrl(p.ImageUrl),
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name
            })
            .ToListAsync();
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
                ImageUrl = _fileUrl.GetFullUrl(p.ImageUrl),
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
                ImageUrl = _fileUrl.GetFullUrl(p.ImageUrl),
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
                ImageUrl = _fileUrl.GetFullUrl(p.ImageUrl),
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

    // Random Products for Home Page
    public async Task<List<object>> GetRandomProducts(int count = 10)
    {
        return await _context.Products
            .OrderBy(r => Guid.NewGuid()) // random
            .Take(count)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Price,
                ImageUrl = _fileUrl.GetFullUrl(p.ImageUrl),

                Category = new
                {
                    p.Category.Id,
                    p.Category.Name
                },

                Store = new
                {
                    p.Category.Store.Id,
                    p.Category.Store.Name
                }
            })
            .ToListAsync<object>();
    }


    //  Get All Products with Pagination
    public async Task<PaginatedResponse<object>> GetAllProducts(PaginationRequest request)
    {
        var query = _context.Products.AsQueryable();

        var totalCount = await query.CountAsync();

        var data = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Price,
                ImageUrl = _fileUrl.GetFullUrl(p.ImageUrl),

                Category = new
                {
                    p.Category.Id,
                    p.Category.Name
                },

                Store = new
                {
                    p.Category.Store.Id,
                    p.Category.Store.Name
                }
            })
            .ToListAsync<object>();

        return new PaginatedResponse<object>
        {
            Page = request.Page,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
            Data = data
        };
    }

    // Get Product With fulter
    public async Task<PaginatedResponse<object>> GetFilteredProducts(
    Guid? storeId,
    Guid? categoryId,
    PaginationRequest request)
    {
        var query = _context.Products.AsQueryable();

        if (storeId != null)
            query = query.Where(p => p.Category.StoreId == storeId);

        if (categoryId != null)
            query = query.Where(p => p.CategoryId == categoryId);

        var totalCount = await query.CountAsync();

        var data = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Price,
                ImageUrl = _fileUrl.GetFullUrl(p.ImageUrl),
                Category = p.Category.Name,
                Store = p.Category.Store.Name
            })
            .ToListAsync<object>();

        return new PaginatedResponse<object>
        {
            Page = request.Page,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
            Data = data
        };
    }

    // Search Products by Name
    public async Task<PaginatedResponse<object>> Search(string text, PaginationRequest request)
    {
        var query = _context.Products
            .Where(p => p.Name.Contains(text));

        var totalCount = await query.CountAsync();

        
        var products = await query
            .Include(p => p.Category)
            .ThenInclude(c => c.Store)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        
        var data = products.Select(p => new
        {
            id = p.Id,
            name = p.Name,
            price = p.Price,
            imageUrl = _fileUrl.GetFullUrl(p.ImageUrl), 
            category = p.Category.Name,
            store = new
            {
                id = p.Category.Store.Id,
                name = p.Category.Store.Name
            }
        }).ToList<object>();

        return new PaginatedResponse<object>
        {
            Page = request.Page,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
            Data = data
        };
    }
}
