using Microsoft.EntityFrameworkCore;
using TahaMarket.Application.DTOs;
using TahaMarket.Domain.Entities;
using TahaMarket.Domain.Enums;
using TahaMarket.Infrastructure.Data;

public class OrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================
    // CREATE ORDER (User)
    // =========================
    public async Task<object> CreateOrder(Guid userId, CreateOrderRequest request)
    {
        var store = await _context.Stores.FindAsync(request.StoreId);
        if (store == null)
            throw new Exception("Store not found");

        var address = await _context.UserAddresses
            .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == userId);

        if (address == null)
            throw new Exception("Invalid address");

        var now = DateTime.UtcNow;

        // -------------------------
        // Load products in batch
        // -------------------------
        var productIds = request.Items.Select(x => x.ProductId).ToList();

        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        if (products.Count != productIds.Count)
            throw new Exception("One or more products not found");

        // -------------------------
        // Load active offers once
        // -------------------------
        var offers = await _context.Offers
            .Where(o =>
                o.IsActive &&
                o.StartDate <= now &&
                o.EndDate >= now)
            .ToListAsync();

        var order = new Order
        {
            UserId = userId,
            StoreId = request.StoreId,
            AddressId = request.AddressId,
            Status = OrderStatus.Pending,
            CreatedAt = now
        };

        decimal total = 0;

        foreach (var item in request.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);

            // base price
            var basePrice = product.Variants.Any()
                ? product.Variants.Min(v => v.Price)
                : throw new Exception("Product has no price");

            // offer logic (product OR category)
            var bestOffer = offers
                .Where(o =>
                    (o.TargetType == OfferTargetType.Product && o.TargetId == product.Id) ||
                    (o.TargetType == OfferTargetType.Category && o.TargetId == product.CategoryId))
                .OrderByDescending(o => o.DiscountPercentage)
                .FirstOrDefault();

            var discount = bestOffer?.DiscountPercentage ?? 0;

            var finalPrice = basePrice;

            if (discount > 0)
                finalPrice = basePrice - (basePrice * discount / 100);

            var itemTotal = finalPrice * item.Quantity;

            total += itemTotal;

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                Price = finalPrice,
                Note = item.Note
            });
        }

        order.TotalPrice = total;
        order.DeliveryFee = 0;
        order.FinalPrice = total + order.DeliveryFee;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return new
        {
            order.Id,
            order.Status,
            order.TotalPrice,
            order.FinalPrice
        };
    }

    // =========================
    // GET USER ORDERS
    // =========================
    public async Task<object> GetUserOrders(Guid userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Store)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Select(o => new
            {
                o.Id,
                Store = o.Store.Name,
                Status = o.Status,
                TotalPrice = o.TotalPrice,
                FinalPrice = o.FinalPrice,
                Items = o.Items.Select(i => new
                {
                    i.Product.Name,
                    i.Quantity,
                    i.Price,
                    i.Note
                }),
                o.CreatedAt
            })
            .ToListAsync();
    }

    // =========================
    // STORE DASHBOARD
    // =========================
    public async Task<object> GetStoreDashboard(Guid storeId)
    {
        var today = DateTime.UtcNow.Date;

        var totalRevenue = await _context.Orders
            .Where(o =>
                o.StoreId == storeId &&
                (o.Status == OrderStatus.Accepted || o.Status == OrderStatus.Delivered) &&
                o.CreatedAt >= today &&
                o.CreatedAt < today.AddDays(1))
            .SumAsync(o => (decimal?)o.FinalPrice) ?? 0;

        var pendingOrders = await _context.Orders
            .CountAsync(o => o.StoreId == storeId && o.Status == OrderStatus.Pending);

        var deliveredOrders = await _context.Orders
            .CountAsync(o => o.StoreId == storeId && o.Status == OrderStatus.Delivered);

        var rejectedOrders = await _context.Orders
            .CountAsync(o => o.StoreId == storeId && o.Status == OrderStatus.Rejected);

        return new
        {
            TotalRevenue = totalRevenue,
            PendingOrders = pendingOrders,
            DeliveredOrders = deliveredOrders,
            RejectedOrders = rejectedOrders
        };
    }

    // =========================
    // STORE GET ORDERS
    // =========================
    public async Task<object> GetStoreOrders(Guid storeId, OrderStatus? status = null)
    {
        var query = _context.Orders
            .Where(o => o.StoreId == storeId)
            .Include(o => o.User)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        return await query
            .Select(o => new
            {
                o.Id,
                User = o.User.Name,
                o.Status,
                o.TotalPrice,
                o.FinalPrice,
                Items = o.Items.Select(i => new
                {
                    Product = i.Product.Name,
                    i.Quantity,
                    i.Price,
                    i.Note
                }),
                o.CreatedAt
            })
            .ToListAsync();
    }

    // =========================
    // ACCEPT ORDER
    // =========================
    public async Task AcceptOrder(Guid orderId, Guid storeId)
    {
        var order = await _context.Orders.FindAsync(orderId);

        if (order == null || order.StoreId != storeId)
            throw new Exception("Order not found");

        if (order.Status != OrderStatus.Pending)
            throw new Exception("Invalid state");

        order.Status = OrderStatus.Accepted;
        order.AcceptedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    // =========================
    // REJECT ORDER
    // =========================
    public async Task RejectOrder(Guid orderId, Guid storeId)
    {
        var order = await _context.Orders.FindAsync(orderId);

        if (order == null || order.StoreId != storeId)
            throw new Exception("Order not found");

        order.Status = OrderStatus.Rejected;

        await _context.SaveChangesAsync();
    }

    // =========================
    // ADMIN GET ALL ORDERS
    // =========================
    public async Task<object> GetAllOrders()
    {
        return await _context.Orders
            .Include(o => o.Store)
            .Include(o => o.User)
            .Select(o => new
            {
                o.Id,
                Store = o.Store.Name,
                User = o.User.Name,
                o.Status,
                o.TotalPrice,
                o.FinalPrice
            })
            .ToListAsync();
    }

    // =========================
    // ADMIN GET ORDERS BY STORE
    // =========================
    public async Task<object> GetOrdersByStore(Guid storeId)
    {
        return await _context.Orders
            .Where(o => o.StoreId == storeId)
            .Include(o => o.User)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Select(o => new
            {
                o.Id,
                User = o.User.Name,
                o.Status,
                o.TotalPrice,
                o.FinalPrice,
                Items = o.Items.Select(i => new
                {
                    Product = i.Product.Name,
                    i.Quantity,
                    i.Price,
                    i.Note
                }),
                o.CreatedAt
            })
            .ToListAsync();
    }

    // =========================
    // ADMIN DASHBOARD
    // =========================
    public async Task<object> GetAdminDashboard()
    {
        var stores = await _context.Stores
            .Include(s => s.Orders)
            .ToListAsync();

        return stores.Select(s => new
        {
            StoreId = s.Id,
            StoreName = s.Name,
            TotalRevenue = s.Orders
                .Where(o => o.Status == OrderStatus.Accepted || o.Status == OrderStatus.Delivered)
                .Sum(o => o.FinalPrice),
            PendingOrders = s.Orders.Count(o => o.Status == OrderStatus.Pending),
            DeliveredOrders = s.Orders.Count(o => o.Status == OrderStatus.Delivered),
            RejectedOrders = s.Orders.Count(o => o.Status == OrderStatus.Rejected),
            TotalOrders = s.Orders.Count
        }).ToList();
    }
}