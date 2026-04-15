using Microsoft.EntityFrameworkCore;
using TahaMarket.Application.DTOs;
using TahaMarket.Domain.Entities;
using TahaMarket.Infrastructure.Data;

namespace TahaMarket.Application.Services
{
    public class CategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // CREATE CATEGORY
        // =========================
        public async Task<Category> Create(Guid storeId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Category name is required");

            var storeExists = await _context.Stores
                .AnyAsync(s => s.Id == storeId);

            if (!storeExists)
                throw new Exception("Store not found");

            var category = new Category
            {
                Name = name,
                StoreId = storeId
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return category;
        }

        // =========================
        // GET ALL BY STORE (PUBLIC)
        // =========================
        public async Task<List<CategoryResponse>> GetByStore(Guid storeId)
        {
            var storeExists = await _context.Stores
                .AnyAsync(s => s.Id == storeId);

            if (!storeExists)
                throw new Exception("Store not found");

            return await _context.Categories
                .Where(c => c.StoreId == storeId)
                .Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }

        // =========================
        // GET BY CATEGORY ID (PUBLIC)
        // =========================
        public async Task<object> GetById(Guid categoryId)
        {
            var category = await _context.Categories
                .Where(c => c.Id == categoryId)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.StoreId
                })
                .FirstOrDefaultAsync();

            if (category == null)
                throw new Exception("Category not found");

            return category;
        }
    }
}