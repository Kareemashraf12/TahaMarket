using Microsoft.EntityFrameworkCore;
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

        public async Task<Category> Create(Guid storeId, string name)
        {
            var category = new Category
            {
                Name = name,
                StoreId = storeId
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return category;
        }

        public async Task<List<Category>> GetMy(Guid storeId)
        {
            return await _context.Categories
                .Where(c => c.StoreId == storeId)
                .ToListAsync();
        }

        public async Task<List<Category>> GetByStore(Guid storeId)
        {
            return await _context.Categories
                .Where(c => c.StoreId == storeId)
                .ToListAsync();
        }
    }
}
