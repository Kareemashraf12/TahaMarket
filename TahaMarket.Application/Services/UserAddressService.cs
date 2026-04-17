using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TahaMarket.Application.DTOs;
using TahaMarket.Domain.Entities;
using TahaMarket.Infrastructure.Data;

public class UserAddressService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _http;

    public UserAddressService(ApplicationDbContext context, IHttpContextAccessor http)
    {
        _context = context;
        _http = http;
    }

    // =========================
    // GET USER ID FROM TOKEN
    // =========================
    private Guid GetUserId()
    {
        var claim = _http.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (claim == null)
            throw new Exception("Unauthorized");

        return Guid.Parse(claim);
    }

    // =========================
    // ADD ADDRESS
    // =========================
    public async Task<object> AddAddress(CreateAddressRequest request)
    {
        var userId = GetUserId();

        if (request.IsDefault)
        {
            var oldDefault = await _context.UserAddresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync();

            foreach (var item in oldDefault)
                item.IsDefault = false;
        }

        var address = new UserAddress
        {
            UserId = userId,
            AddressType = request.AddressType,
            City = request.City,
            Area = request.Area,
            Street = request.Street,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsDefault = request.IsDefault
        };

        _context.UserAddresses.Add(address);
        await _context.SaveChangesAsync();

        return address;
    }

    // =========================
    // GET MY ADDRESSES
    // =========================
    public async Task<object> GetMyAddresses()
    {
        var userId = GetUserId();

        return await _context.UserAddresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ToListAsync();
    }

    // =========================
    // UPDATE ADDRESS
    // =========================
    public async Task<object> UpdateAddress(UpdateAddressRequest request)
    {
        var userId = GetUserId();

        var address = await _context.UserAddresses
            .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == userId);

        if (address == null)
            throw new Exception("Address not found");

        if (request.IsDefault == true)
        {
            var oldDefault = await _context.UserAddresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync();

            foreach (var item in oldDefault)
                item.IsDefault = false;
        }

        address.AddressType = request.AddressType ?? address.AddressType;
        address.City = request.City ?? address.City;
        address.Area = request.Area ?? address.Area;
        address.Street = request.Street ?? address.Street;
        address.Latitude = request.Latitude ?? address.Latitude;
        address.Longitude = request.Longitude ?? address.Longitude;

        if (request.IsDefault.HasValue)
            address.IsDefault = request.IsDefault.Value;

        await _context.SaveChangesAsync();

        return address;
    }

    // =========================
    // DELETE ADDRESS
    // =========================
    public async Task DeleteAddress(Guid addressId)
    {
        var userId = GetUserId();

        var address = await _context.UserAddresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);

        if (address == null)
            throw new Exception("Address not found");

        _context.UserAddresses.Remove(address);
        await _context.SaveChangesAsync();
    }
}