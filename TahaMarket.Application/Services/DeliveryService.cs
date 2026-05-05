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

            //  NEW STATE MODEL
            Status = DeliveryStatus.Offline
        };

        _context.Deliveries.Add(delivery);
        await _context.SaveChangesAsync();

        return new
        {
            delivery.Id,
            delivery.Name,
            delivery.PhoneNumber,
            delivery.VehicleType,
            Status = delivery.Status.ToString(),
            ImageUrl = _fileUrl.GetFullUrl(delivery.ImageUrl),
            delivery.Balance
        };
    }


    // =========================
    // ASSIGN ORDER (AUTO)
    // =========================
    public async Task AssignOrder(Guid orderId, Guid deliveryId)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var role = _httpContextAccessor.HttpContext?
                    .User.FindFirst(ClaimTypes.Role)?.Value;

                if (role != "Admin")
                    throw new Exception("Not allowed to assign delivery");

                var order = await _context.Orders
                    .Include(o => o.Store)
                    .Include(o => o.UserAddress)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    throw new Exception("Order not found");

                if (order.PaymentStatus != PaymentStatus.Paid)
                    throw new Exception("Order must be paid");

                if (order.Status == OrderStatus.Assigned)
                    throw new Exception("Order already assigned");

                var alreadyAssigned = await _context.DeliveryOrders
                    .AnyAsync(d => d.OrderId == orderId);

                if (alreadyAssigned)
                    throw new Exception("Order already assigned");

                var delivery = await _context.Deliveries
                    .FirstOrDefaultAsync(d => d.Id == deliveryId);

                if (delivery == null)
                    throw new Exception("Delivery not found");

                if (delivery.Status != DeliveryStatus.Online)
                    throw new Exception("Delivery is not available");

                var deliveryOrder = new DeliveryOrder
                {
                    OrderId = orderId,
                    DeliveryId = deliveryId,
                    Status = DeliveryOrderStatus.Pending,
                    DeliveryFee = 0
                };

                _context.DeliveryOrders.Add(deliveryOrder);

                order.Status = OrderStatus.Assigned;

                delivery.Status = DeliveryStatus.Busy;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
    // =========================
    // Picked ORDER
    // =========================
    public async Task PickOrder(Guid orderId)
    {
        var deliveryId = GetDeliveryIdFromToken();

        var deliveryOrder = await _context.DeliveryOrders
            .Include(d => d.Delivery)
            .FirstOrDefaultAsync(d =>
                d.OrderId == orderId &&
                d.DeliveryId == deliveryId);

        if (deliveryOrder == null)
            throw new Exception("Not assigned to you");

        if (deliveryOrder.Status != DeliveryOrderStatus.Pending)
            throw new Exception("Invalid state");

        deliveryOrder.Status = DeliveryOrderStatus.Picked;
        deliveryOrder.PickedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    // =========================
    // DELIVER ORDER
    // ========================
    public async Task DeliverOrder(Guid orderId)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var deliveryId = GetDeliveryIdFromToken();

                var deliveryOrder = await _context.DeliveryOrders
                    .Include(d => d.Delivery)
                    .Include(d => d.Order)
                    .FirstOrDefaultAsync(d =>
                        d.OrderId == orderId &&
                        d.DeliveryId == deliveryId);

                if (deliveryOrder == null)
                    throw new Exception("Not assigned to you");

                if (deliveryOrder.Status != DeliveryOrderStatus.Picked)
                    throw new Exception("Order not picked yet");

                // =========================
                // UPDATE DELIVERY ORDER
                // =========================
                deliveryOrder.Status = DeliveryOrderStatus.Delivered;
                deliveryOrder.DeliveredAt = DateTime.UtcNow;

                // =========================
                // UPDATE ORDER
                // =========================
                deliveryOrder.Order.Status = OrderStatus.Delivered;
                deliveryOrder.Order.DeliveredAt = DateTime.UtcNow;

                // =========================
                // FREE DELIVERY
                // =========================
                deliveryOrder.Delivery.Status = DeliveryStatus.Online;

                // =========================
                // EARNINGS
                // =========================
                deliveryOrder.Delivery.Balance += deliveryOrder.DeliveryFee;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
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

    // ========================
    // GET AVAILABLE DELIVERIES (ADMIN)
    // ========================
    public async Task<object> GetAvailableDeliveries()
    {
        return await _context.Deliveries
            .AsNoTracking()
            .Where(d => d.Status == DeliveryStatus.Online)
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.VehicleType
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
            delivery.Status,
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
            delivery.Status,
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
        delivery.Status = DeliveryStatus.Offline;
        if (delivery == null)
            throw new Exception("Delivery not found");

        delivery.RefreshToken = null;
        delivery.RefreshTokenExpiry = null;

        await _context.SaveChangesAsync();
    }
}