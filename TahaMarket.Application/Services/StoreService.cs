using Microsoft.EntityFrameworkCore;
using TahaMarket.Application.DTOs;
using TahaMarket.Domain.Entities;
using TahaMarket.Infrastructure.Data;

namespace TahaMarket.Application.Services
{
    public class StoreService
    {
        private readonly ApplicationDbContext _context;

        public StoreService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Store> CreateStore(CreateStoreRequest request)
        {
            var exists = await _context.Stores
            .AnyAsync(s => s.PhoneNumber == request.PhoneNumber);

            if (exists)
                throw new Exception("Store already exists");
            var fileName = Guid.NewGuid() + Path.GetExtension(request.Image.FileName);

            var path = Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot/images/stores", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await request.Image.CopyToAsync(stream);
            }

            var store = new Store
            {
                Name = request.Name,
                Address = request.Address,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                MinimumOrderAmount = request.MinimumOrderAmount,
                Type = request.Type,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                ImageUrl = "/images/stores/" + fileName 
            };

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            return store;
        }

        // 🔥 Store profile
        public async Task<Store> GetMyStore(Guid storeId)
        {
            return await _context.Stores
                .FirstOrDefaultAsync(s => s.Id == storeId);
        }

        // 🔥 Update store
        //public async Task UpdateStore(Guid storeId, UpdateStoreRequest request)
        //{
        //    var store = await _context.Stores.FindAsync(storeId);

        //    if (store == null)
        //        throw new Exception("Store not found");

        //    store.Name = request.Name;
        //    store.Address = request.Address;
        //    store.PhoneNumber = request.PhoneNumber;

        //    await _context.SaveChangesAsync();
        //}

        public async Task<List<Store>> GetAll()
        {
            return await _context.Stores.ToListAsync();
        }
    }
}
