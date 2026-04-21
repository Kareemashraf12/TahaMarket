using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services.Common;
using TahaMarket.Domain.Entities;
using TahaMarket.Domain.Enums;
using TahaMarket.Infrastructure.Data;

public class DeliveryService
{
    private readonly ApplicationDbContext _context;
    private readonly FileUrlService _fileUrl;
    private readonly DistanceService _distance;
    private readonly DeliveryPricingService _pricing;
    private readonly PaymentService _payment;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeliveryService(
        ApplicationDbContext context,
        FileUrlService fileUrl,
        DistanceService distance,
        DeliveryPricingService pricing,
        PaymentService payment,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _fileUrl = fileUrl;
        _distance = distance;
        _pricing = pricing;
        _payment = payment;
        _httpContextAccessor = httpContextAccessor;
    }

    // =========================
    // GET DELIVERY ID FROM TOKEN
    // =========================
    private Guid GetDeliveryIdFromToken()
    {
        var claim = _httpContextAccessor.HttpContext?
            .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (claim == null)
            throw new Exception("Delivery not authenticated");

        return Guid.Parse(claim);
    }

    // =========================
    // CREATE DELIVERY (ADMIN ONLY)
    // =========================
    public async Task<object> Create(CreateDeliveryRequest request)
    {
        var exists = await _context.Deliveries
            .AnyAsync(d => d.PhoneNumber == request.PhoneNumber);

        if (exists)
            throw new Exception("Delivery already exists");

        var delivery = new Delivery
        {
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            ImageUrl = "/images/deliveries/default.png",
            Balance = 0,
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
            delivery.Balance
        };
    }


    // =========================
    // ASSIGN ORDER (AUTO)
    // =========================
    public async Task AssignOrder(Guid orderId, Guid deliveryId)
    {
        // =========================
        // GET USER TYPE FROM TOKEN
        // =========================
        var role = _httpContextAccessor.HttpContext?
            .User.FindFirst(ClaimTypes.Role)?.Value;

        if (role != "Admin" && role != "Store")
            throw new Exception("Not allowed to assign delivery");

        // =========================
        // GET ORDER
        // =========================
        var order = await _context.Orders
            .Include(o => o.Store)
            .Include(o => o.UserAddress)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new Exception("Order not found");

        // must be payed
        if (order.PaymentStatus != PaymentStatus.Paid)
            throw new Exception("Order must be paid before delivery");

        // =========================
        // GET DELIVERY
        // =========================
        var delivery = await _context.Deliveries.FindAsync(deliveryId);

        if (delivery == null)
            throw new Exception("Delivery not found");

        if (!delivery.IsAvailable)
            throw new Exception("Delivery not available");

        // =========================
        // CHECK ASSIGNED
        // =========================
        var alreadyAssigned = await _context.DeliveryOrders
            .AnyAsync(d => d.OrderId == orderId);

        if (alreadyAssigned)
            throw new Exception("Order already assigned");

        // =========================
        // CALCULATE DISTANCE + FEE
        // =========================
        var distanceKm = _distance.CalculateDistanceKm(
            order.Store.Latitude,
            order.Store.Longitude,
            order.UserAddress.Latitude,
            order.UserAddress.Longitude
        );

        var finalFee = await _pricing.CalculateFee(distanceKm);

        // =========================
        // CREATE DELIVERY ORDER
        // =========================
        var deliveryOrder = new DeliveryOrder
        {
            OrderId = orderId,
            DeliveryId = deliveryId,
            DeliveryFee = finalFee,
            Status = DeliveryStatus.Pending
        };

        _context.DeliveryOrders.Add(deliveryOrder);

        // =========================
        // UPDATE ORDER
        // =========================
        order.Status = OrderStatus.Assigned;

        await _context.SaveChangesAsync();
    }

    // =========================
    // ACCEPT ORDER
    // =========================
    public async Task AcceptOrder(Guid orderId)
    {
        var deliveryId = GetDeliveryIdFromToken();

        var deliveryOrder = await _context.DeliveryOrders
            .FirstOrDefaultAsync(d =>
                d.OrderId == orderId &&
                d.DeliveryId == deliveryId);

        if (deliveryOrder == null)
            throw new Exception("Not assigned");

        if (deliveryOrder.Status != DeliveryStatus.Pending)
            throw new Exception("Invalid state");

        deliveryOrder.Status = DeliveryStatus.Picked;
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
            .Include(d => d.Order)
            .FirstOrDefaultAsync(d =>
                d.OrderId == orderId &&
                d.DeliveryId == deliveryId);

        if (deliveryOrder == null)
            throw new Exception("Not assigned");

        if (deliveryOrder.Status != DeliveryStatus.Picked)
            throw new Exception("Invalid state");

        deliveryOrder.Status = DeliveryStatus.Delivered;
        deliveryOrder.DeliveredAt = DateTime.UtcNow;

        //  update order
        deliveryOrder.Order.Status = OrderStatus.Delivered;
        deliveryOrder.Order.DeliveredAt = DateTime.UtcNow;

        // delivery earns money
        deliveryOrder.Delivery.Balance += deliveryOrder.DeliveryFee;

        await _context.SaveChangesAsync();
    }


    // =========================
    // GET MY ORDERS (DELIVERY)
    // =========================
    public async Task<object> GetMyOrders()
    {
        var deliveryId = GetDeliveryIdFromToken();

        return await _context.DeliveryOrders
            .AsNoTracking()
            .Include(d => d.Order)
            .ThenInclude(o => o.Store)
            .Where(d => d.DeliveryId == deliveryId)
            .Select(d => new
            {
                d.OrderId,
                d.Status,
                d.DeliveryFee,
                Store = d.Order.Store.Name,
                d.Order.CreatedAt
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

        if (delivery == null)
            throw new Exception("Delivery not found");

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

        if (delivery == null)
            throw new Exception("Delivery not found");

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

        if (delivery == null)
            throw new Exception("Delivery not found");

        delivery.RefreshToken = null;
        delivery.RefreshTokenExpiry = null;

        await _context.SaveChangesAsync();
    }
}