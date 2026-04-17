using Microsoft.EntityFrameworkCore;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services.Common;
using TahaMarket.Domain.Enums;
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
            // =========================
            // CHECK PHONE EXISTS
            // =========================
            var exists = await _context.Stores
                .AnyAsync(s => s.PhoneNumber == request.PhoneNumber);

            if (exists)
                throw new Exception("Store already exists");

            // =========================
            // VALIDATE STORE SECTION
            // =========================
            var sectionExists = await _context.storeSections
                .AnyAsync(s => s.Id == request.StoreSectionId);

            if (!sectionExists)
                throw new Exception("Invalid StoreSectionId");

            // =========================
            // HANDLE IMAGE
            // =========================
            string imagePath = "/images/stores/default.png";

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

                imagePath = "/images/stores/" + fileName;
            }

            // =========================
            // CREATE STORE
            // =========================
            var store = new Store
            {
                Name = request.Name,
                Address = request.Address,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                MinimumOrderAmount = request.MinimumOrderAmount,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                ImageUrl = imagePath,

                Description = request.Description,
                OpenTime = request.TimeOpen,
                CloseTime = request.TimeClose,
                StoreSectionId = request.StoreSectionId
            };

            // =========================
            // SAVE WITH ERROR HANDLING
            // =========================
            try
            {
                _context.Stores.Add(store);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new Exception("Error while saving store. Please check data integrity.");
            }

            // =========================
            // RETURN RESPONSE
            // =========================
            return await MapToResponse(store);
        }
        // =========================
        // GET ALL (PUBLIC)
        // =========================
        public async Task<List<StoreResponse>> GetAll()
        {
            return await _context.Stores
                .Select(s => new StoreResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    ImageUrl = _fileUrl.GetFullUrl(s.ImageUrl),

                    AverageRating = _context.Ratings
                        .Where(r => r.TargetId == s.Id && r.TargetType == RatingTargetType.Store)
                        .Average(r => (double?)r.Value) ?? 0
                })
                .ToListAsync();
        }

        // =========================
        // GET BY ID (FULL DETAILS)
        // =========================
        public async Task<object> GetById(Guid id)
        {
            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.Id == id);

            if (store == null)
                throw new Exception("Store not found");

            var average = await _context.Ratings
                .Where(r => r.TargetId == id && r.TargetType == RatingTargetType.Store)
                .AverageAsync(r => (double?)r.Value) ?? 0;

            return new
            {
                store.Id,
                store.Name,
                store.Description,
                store.Address,
                store.PhoneNumber,
                store.MinimumOrderAmount,
                store.Latitude,
                store.Longitude,
                ImageUrl = _fileUrl.GetFullUrl(store.ImageUrl),

                store.OpenTime,
                store.CloseTime,
                store.StoreSectionId,

                AverageRating = average
            };
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

            store.Description = request.Description ?? store.Description;
            store.OpenTime = request.OpenTime ?? store.OpenTime;
            store.CloseTime = request.CloseTime ?? store.CloseTime;

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

            return await MapToResponse(store);
        }

        // =========================
        // DELETE STORE
        // =========================
        public async Task DeleteStore(Guid id)
        {
            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.Id == id);

            if (store == null)
                throw new Exception("Store not found");

            _context.Stores.Remove(store);
            await _context.SaveChangesAsync();
        }

        // =========================
        // MAPPER
        // =========================
        private async Task<StoreResponse> MapToResponse(Store store)
        {
            var average = await _context.Ratings
                .Where(r => r.TargetId == store.Id && r.TargetType == RatingTargetType.Store)
                .AverageAsync(r => (double?)r.Value) ?? 0;

            return new StoreResponse
            {
                Id = store.Id,
                Name = store.Name,
                Description = store.Description,
                ImageUrl = _fileUrl.GetFullUrl(store.ImageUrl),
                AverageRating = average
            };
        }
    }
}