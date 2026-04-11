using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services.Common;
using TahaMarket.Domain.Entities;
using TahaMarket.Infrastructure.Data;

public class DeliveryService
{
    private readonly ApplicationDbContext _context;
    private readonly FileUrlService _fileUrl;
    private readonly DistanceService _distance;
    private readonly DeliveryPricingService _pricing;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeliveryService(
        ApplicationDbContext context,
        FileUrlService fileUrl,
        DistanceService distance,
        DeliveryPricingService pricing,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _fileUrl = fileUrl;
        _distance = distance;
        _pricing = pricing;
        _httpContextAccessor = httpContextAccessor;
    }

    // Helper to get DeliveryId from token
    private Guid GetDeliveryIdFromToken()
    {
        var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (claim == null) throw new Exception("Delivery not authenticated");
        return Guid.Parse(claim);
    }

    // =========================
    // CREATE DELIVERY (Admin)
    // =========================
    public async Task<object> Create(CreateDeliveryRequest request)
    {

        var exists = await _context.Deliveries
            .AnyAsync(d => d.PhoneNumber == request.PhoneNumber);

        if (exists)
            throw new Exception("Delivery already exists");

        
        string imageUrl = "/images/deliveries/delivery.png";

        var delivery = new Delivery
        {
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            ImageUrl = imageUrl,
            Balance = 0,
            RefreshToken = "",                      
            RefreshTokenExpiry = DateTime.UtcNow, 
            VehicleType = request.VehicleType,                      
            IsAvailable = true,                   
            IsOnline = false                      
        };

        _context.Deliveries.Add(delivery);
        await _context.SaveChangesAsync();

           
        return new
        {
            delivery.Id,
            delivery.Name,
            delivery.PhoneNumber,
            delivery.VehicleType,
            delivery.IsAvailable,
            delivery.IsOnline,
            ImageUrl = _fileUrl.GetFullUrl(delivery.ImageUrl),
            delivery.Balance,
            delivery.RefreshToken,
            delivery.RefreshTokenExpiry
        };
    }

    // =========================
    // ASSIGN ORDER
    // =========================
    public async Task AssignOrder(Guid orderId, Guid deliveryId)
    {
        var order = await _context.Orders
            .Include(o => o.Store)
            .Include(o => o.UserAddress)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) throw new Exception("Order not found");

        var delivery = await _context.Deliveries.FindAsync(deliveryId);
        if (delivery == null) throw new Exception("Delivery not found");

        var distance = _distance.CalculateDistance(
            order.Store.Latitude,
            order.Store.Longitude,
            order.UserAddress.Latitude,
            order.UserAddress.Longitude
        );

        var fee = _pricing.CalculateFee(distance);

        var deliveryOrder = new DeliveryOrder
        {
            OrderId = orderId,
            DeliveryId = deliveryId,
            DeliveryFee = fee,
            Status = "Pending"
        };

        _context.DeliveryOrders.Add(deliveryOrder);
        await _context.SaveChangesAsync();
    }

    // =========================
    // ACCEPT ORDER
    // =========================
    public async Task AcceptOrder(Guid orderId)
    {
        var deliveryId = GetDeliveryIdFromToken();

        var deliveryOrder = await _context.DeliveryOrders
            .FirstOrDefaultAsync(d => d.OrderId == orderId && d.DeliveryId == deliveryId);

        if (deliveryOrder == null) throw new Exception("Not assigned");
        if (deliveryOrder.Status != "Pending") throw new Exception("Invalid state");

        deliveryOrder.Status = "Picked";
        deliveryOrder.PickedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    // =========================
    // DELIVER ORDER
    // =========================
    public async Task DeliverOrder(Guid orderId)
    {
        var deliveryId = GetDeliveryIdFromToken();

        var deliveryOrder = await _context.DeliveryOrders
            .Include(d => d.Delivery)
            .FirstOrDefaultAsync(d => d.OrderId == orderId && d.DeliveryId == deliveryId);

        if (deliveryOrder == null) throw new Exception("Not assigned");
        if (deliveryOrder.Status != "Picked") throw new Exception("Invalid state");

        deliveryOrder.Status = "Delivered";
        deliveryOrder.DeliveredAt = DateTime.UtcNow;
        deliveryOrder.Delivery.Balance += deliveryOrder.DeliveryFee;

        await _context.SaveChangesAsync();
    }

    // =========================
    // GET MY ORDERS
    // =========================
    public async Task<object> GetMyOrders()
    {
        var deliveryId = GetDeliveryIdFromToken();

        return await _context.DeliveryOrders
            .Include(d => d.Order)
            .ThenInclude(o => o.Store)
            .Where(d => d.DeliveryId == deliveryId)
            .Select(d => new
            {
                d.OrderId,
                d.Status,
                d.DeliveryFee,
                Store = d.Order.Store.Name,
                CreatedAt = d.Order.CreatedAt
            })
            .ToListAsync();
    }

    // =========================
    // GET PROFILE
    // =========================
    public async Task<object> GetProfile()
    {
        var deliveryId = GetDeliveryIdFromToken();

        var delivery = await _context.Deliveries.FindAsync(deliveryId);
        if (delivery == null) throw new Exception("Delivery not found");

        return new
        {
            delivery.Id,
            delivery.Name,
            delivery.PhoneNumber,
            delivery.VehicleType,
            delivery.IsAvailable,
            delivery.IsOnline,
            delivery.CurrentLatitude,
            delivery.CurrentLongitude,
            ImageUrl = _fileUrl.GetFullUrl(delivery.ImageUrl),
            delivery.Balance
        };
    }

    // =========================
    // UPDATE PROFILE
    // =========================
    public async Task<object> UpdateProfile(UpdateDeliveryProfileRequest request)
    {
        var deliveryId = GetDeliveryIdFromToken();

        var delivery = await _context.Deliveries.FindAsync(deliveryId);
        if (delivery == null) throw new Exception("Delivery not found");

        delivery.Name = request.Name;
        if (!string.IsNullOrEmpty(request.VehicleType))
            delivery.VehicleType = request.VehicleType;

        if (request.Image != null)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(request.Image.FileName);
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/deliveries");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var path = Path.Combine(folder, fileName);
            using (var stream = new FileStream(path, FileMode.Create))
                await request.Image.CopyToAsync(stream);

            delivery.ImageUrl = "/images/deliveries/" + fileName;
        }

        await _context.SaveChangesAsync();

        return new
        {
            delivery.Id,
            delivery.Name,
            delivery.PhoneNumber,
            delivery.VehicleType,
            delivery.IsAvailable,
            delivery.IsOnline,
            ImageUrl = _fileUrl.GetFullUrl(delivery.ImageUrl),
            delivery.Balance
        };
    }

    // =========================
    // LOGOUT
    // =========================
    public async Task Logout()
    {
        var deliveryId = GetDeliveryIdFromToken();
        var delivery = await _context.Deliveries.FindAsync(deliveryId);
        if (delivery == null) throw new Exception("Delivery not found");

        delivery.RefreshToken = null;
        delivery.RefreshTokenExpiry = null;

        await _context.SaveChangesAsync();
    }
}