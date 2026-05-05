using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TahaMarket.Application.DTOs;
using TahaMarket.Domain.Entities;
using TahaMarket.Infrastructure.Data;

public class AddOnService
{
    private readonly ApplicationDbContext _context;

    public AddOnService(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================
    // CREATE GROUP WITH OPTIONS (ATOMIC)
    // =========================
    public async Task<ApiResponse> CreateGroupWithOptions(CreateAddOnGroupWithOptionsRequest request)
    {
        // =========================
        // VALIDATION
        // =========================
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new Exception("Group name is required");

        if (request.ProductId == null && request.StoreId == null)
            throw new Exception("ProductId or StoreId is required");

        if (request.ProductId != null && request.StoreId != null)
            throw new Exception("Cannot assign to both Product and Store");

        if (request.Options == null || !request.Options.Any())
            throw new Exception("At least one option is required");

        if (request.Options.Any(o => o.Price < 0))
            throw new Exception("Invalid option price");

        // =========================
        // EXECUTION STRATEGY WRAPPER (FIX)
        // =========================
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            // =========================
            // TRANSACTION
            // =========================
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // =========================
                // CREATE GROUP
                // =========================
                var group = new AddOnGroup
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    ProductId = request.ProductId,
                    StoreId = request.StoreId,
                    IsActive = true
                };

                _context.AddOnGroups.Add(group);
                await _context.SaveChangesAsync();

                // =========================
                // CREATE OPTIONS
                // =========================
                var options = request.Options.Select(o => new AddOnOption
                {
                    Id = Guid.NewGuid(),
                    Name = o.Name,
                    Price = o.Price,
                    AddOnGroupId = group.Id,
                    IsActive = true
                }).ToList();

                _context.AddOnOptions.AddRange(options);
                await _context.SaveChangesAsync();

                // =========================
                // COMMIT
                // =========================
                await transaction.CommitAsync();

                return new ApiResponse
                {
                    Success = true,
                    Message = "تم إضافة الإضافات بنجاح",
                    
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    // =========================
    // GET ACTIVE GROUPS WITH OPTIONS BY PRODUCT OR STORE
    // =========================
    public async Task<List<AddOnGroupFullDto>> Get(Guid? productId, Guid? storeId)
    {
        // =========================
        // VALIDATION
        // =========================
        if (productId == null && storeId == null)
            throw new Exception("Either productId or storeId is required");

        if (productId != null && storeId != null)
            throw new Exception("Send only productId OR storeId, not both");

        // =========================
        // QUERY BASE
        // =========================
        var query = _context.AddOnGroups
            .AsNoTracking()
            .Where(g => g.IsActive);

        // =========================
        // FILTER
        // =========================
        if (productId != null)
        {
            query = query.Where(g => g.ProductId == productId);
        }
        else
        {
            query = query.Where(g => g.StoreId == storeId);
        }

        // =========================
        // PROJECT
        // =========================
        var result = await query
            .Select(g => new AddOnGroupFullDto
            {
                Id = g.Id,
                Name = g.Name,
                ProductId = g.ProductId,
                StoreId = g.StoreId,

                Options = g.Options
                    .Where(o => o.IsActive)
                    .Select(o => new AddOnOptionDto
                    {
                        Id = o.Id,
                        Name = o.Name,
                        Price = o.Price
                    })
                    .ToList()
            })
            .ToListAsync();

        return result; 
    }



    // =========================
    // DELETE OPTION (SOFT DELETE)
    // =========================
    public async Task DeleteOption(Guid optionId)
    {
        var option = await _context.AddOnOptions
            .FirstOrDefaultAsync(o => o.Id == optionId);

        if (option == null)
            throw new Exception("Option not found");

        option.IsActive = false;

        await _context.SaveChangesAsync();
    }

    // =========================================
    // DELETE GROUP WITH OPTIONS (SOFT DELETE)
    // =========================================
    public async Task DeleteGroup(Guid groupId)
    {
        var group = await _context.AddOnGroups
            .Include(g => g.Options)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            throw new Exception("AddOnGroup not found");

        // =========================
        // TRANSACTION SAFETY
        // =========================
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // =========================
            // DELETE OPTIONS FIRST
            // =========================
            if (group.Options != null && group.Options.Any())
            {
                _context.AddOnOptions.RemoveRange(group.Options);
            }

            // =========================
            // DELETE GROUP
            // =========================
            _context.AddOnGroups.Remove(group);

            await _context.SaveChangesAsync();

            // =========================
            // COMMIT
            // =========================
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}