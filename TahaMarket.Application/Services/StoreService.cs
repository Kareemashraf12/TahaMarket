using Microsoft.EntityFrameworkCore;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.DTOs.TahaMarket.Application.DTOs;
using TahaMarket.Application.Services.Common;
using TahaMarket.Domain.Entities;
using TahaMarket.Infrastructure.Data;

namespace TahaMarket.Application.Services
{
    public class StoreService
    {
        private readonly ApplicationDbContext _context;
        private readonly FileUrlService _fileUrl;

        public StoreService(ApplicationDbContext context, FileUrlService fileUrl)
        {
            _context = context;
            _fileUrl = fileUrl;
        }

        // =========================
        // CREATE STORE
        // =========================
        public async Task<StoreResponse> CreateStore(CreateStoreRequest request)
        {
            var exists = await _context.Stores
                .AnyAsync(s => s.PhoneNumber == request.PhoneNumber);

            if (exists)
                throw new Exception("Store already exists");

            var fileName = Guid.NewGuid() + Path.GetExtension(request.Image.FileName);

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/stores");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var path = Path.Combine(folderPath, fileName);

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
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                ImageUrl = "/images/stores/" + fileName
            };

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            return MapToResponse(store);
        }

        // =========================
        // GET MY STORE
        // =========================
        public async Task<StoreResponse> GetMyStore(Guid storeId)
        {
            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.Id == storeId);

            if (store == null)
                throw new Exception("Store not found");

            return MapToResponse(store);
        }

        // =========================
        // GET ALL STORES
        // =========================
        public async Task<List<StoreResponse>> GetAll()
        {
            var stores = await _context.Stores.ToListAsync();

            return stores.Select(s => MapToResponse(s)).ToList();
        }

        // =========================
        // GET BY ID
        // =========================
        public async Task<StoreResponse> GetById(Guid id)
        {
            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.Id == id);

            if (store == null)
                throw new Exception("Store not found");

            return MapToResponse(store);
        }

        // =========================
        // UPDATE STORE
        // =========================
        public async Task<StoreResponse> UpdateStore(Guid storeId, UpdateStoreRequest request)
        {
            var store = await _context.Stores.FindAsync(storeId);

            if (store == null)
                throw new Exception("Store not found");

            store.Name = request.Name ?? store.Name;
            store.Address = request.Address ?? store.Address;
            store.PhoneNumber = request.PhoneNumber ?? store.PhoneNumber;

            if (request.Image != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(request.Image.FileName);

                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/stores");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var path = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await request.Image.CopyToAsync(stream);
                }

                store.ImageUrl = "/images/stores/" + fileName;
            }

            await _context.SaveChangesAsync();

            return MapToResponse(store);
        }

        // =========================
        // MAPPER 
        // =========================
        private StoreResponse MapToResponse(Store store)
        {
            return new StoreResponse
            {
                Id = store.Id,
                Name = store.Name,
                Address = store.Address,
                PhoneNumber = store.PhoneNumber,
                MinimumOrderAmount = store.MinimumOrderAmount,
                
                Latitude = store.Latitude,
                Longitude = store.Longitude,
                ImageUrl = _fileUrl.GetFullUrl(store.ImageUrl)
            };
        }
    }
}