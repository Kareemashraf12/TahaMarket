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
        var now = DateTime.UtcNow;

        // =========================
        // VALIDATION
        // =========================
        var store = await _context.Stores.FindAsync(request.StoreId)
            ?? throw new Exception("Store not found");

        var address = await _context.UserAddresses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == userId)
            ?? throw new Exception("Invalid address");

        var productIds = request.Items.Select(x => x.ProductId).ToList();

        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        if (products.Count != productIds.Count)
            throw new Exception("One or more products not found");

        var offers = await _context.Offers
            .AsNoTracking()
            .Where(o => o.IsActive && o.StartDate <= now && o.EndDate >= now)
            .ToListAsync();

        // =========================
        // PAYMENT LOGIC
        // =========================
        var paymentMethod = request.PaymentMethod;

        var isCOD = paymentMethod == PaymentMethod.COD;

        var order = new Order
        {
            UserId = userId,
            StoreId = request.StoreId,
            AddressId = request.AddressId,

            PaymentMethod = paymentMethod,

            PaymentStatus = isCOD ? PaymentStatus.Paid : PaymentStatus.Pending,
            Status = isCOD ? OrderStatus.Paid : OrderStatus.Pending,

            CreatedAt = now
        };

        decimal total = 0;

        // =========================
        // ITEMS CALCULATION
        // =========================
        foreach (var item in request.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);

            var basePrice = product.Variants.Min(v => v.Price);

            var bestOffer = offers
                .Where(o =>
                    (o.TargetType == OfferTargetType.Product && o.TargetId == product.Id) ||
                    (o.TargetType == OfferTargetType.Category && o.TargetId == product.CategoryId))
                .OrderByDescending(o => o.DiscountPercentage)
                .FirstOrDefault();

            var finalPrice = bestOffer != null
                ? basePrice - (basePrice * bestOffer.DiscountPercentage / 100)
                : basePrice;

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

        // =========================
        // FINAL CALCULATION
        // =========================
        order.TotalPrice = total;
        order.DeliveryFee = 0;
        order.FinalPrice = total;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return new
        {
            order.Id,
            Status = order.Status.ToString(),
            PaymentStatus = order.PaymentStatus.ToString(),
            PaymentMethod = order.PaymentMethod.ToString(),
            order.TotalPrice,
            order.FinalPrice,
            CreatedAt = order.CreatedAt
        };
    }

    // =========================
    // STORE DASHBOARD (UPDATED)
    // =========================
    public async Task<object> GetStoreDashboard(Guid storeId)
    {
        var today = DateTime.UtcNow.Date;

        var result = await _context.Orders
            .Where(o => o.StoreId == storeId)
            .GroupBy(o => 1)
            .Select(g => new
            {
                TotalRevenue = g
                    .Where(o =>
                        o.Status == OrderStatus.Delivered &&
                        o.CreatedAt >= today &&
                        o.CreatedAt < today.AddDays(1))
                    .Sum(o => (decimal?)o.FinalPrice) ?? 0,

                PendingOrders = g.Count(o => o.Status == OrderStatus.Pending),
                PaidOrders = g.Count(o => o.Status == OrderStatus.Paid),
                PreparingOrders = g.Count(o => o.Status == OrderStatus.Preparing),
                ReadyOrders = g.Count(o => o.Status == OrderStatus.Ready),
                AssignedOrders = g.Count(o => o.Status == OrderStatus.Assigned),

                
                OutForDeliveryOrders = g.Count(o => o.Status == OrderStatus.Picked),

                DeliveredOrders = g.Count(o => o.Status == OrderStatus.Delivered),
                CancelledOrders = g.Count(o => o.Status == OrderStatus.Cancelled)
            })
            .FirstOrDefaultAsync();

        // if no orders found
        if (result == null)
        {
            return new
            {
                TotalRevenue = 0,
                PendingOrders = 0,
                PaidOrders = 0,
                PreparingOrders = 0,
                ReadyOrders = 0,
                AssignedOrders = 0,
                OutForDeliveryOrders = 0,
                DeliveredOrders = 0,
                CancelledOrders = 0
            };
        }

        return result;
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
    // UPDATE ORDER STATUS (STORE)
    // =========================
    public async Task UpdateOrderStatus(Guid orderId, Guid storeId, OrderStatus newStatus)
    {
        var order = await _context.Orders.FindAsync(orderId);

        if (order == null || order.StoreId != storeId)
            throw new Exception("Order not found");

        // =========================
        // BLOCK FINISHED ORDERS
        // =========================
        if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
            throw new Exception("Order already completed");

        // =========================
        // VALID TRANSITIONS
        // =========================
        var valid = order.Status switch
        {
            OrderStatus.Paid => new[] { OrderStatus.Preparing },
            OrderStatus.Preparing => new[] { OrderStatus.Ready },
            OrderStatus.Ready => new[] { OrderStatus.Assigned },
            OrderStatus.Assigned => new[] { OrderStatus.Picked },
            OrderStatus.Picked => new[] { OrderStatus.Delivered },

            _ => Array.Empty<OrderStatus>()
        };

        if (!valid.Contains(newStatus))
            throw new Exception($"Invalid transition from {order.Status} to {newStatus}");

        // =========================
        // APPLY STATUS
        // =========================
        order.Status = newStatus;

        // =========================
        // TIMESTAMPS
        // =========================
        var now = DateTime.UtcNow;

        switch (newStatus)
        {
            case OrderStatus.Preparing:
                order.AcceptedAt = now;
                break;

            case OrderStatus.Ready:
                order.ReadyAt = now;
                break;

            case OrderStatus.Assigned:
                order.AssignedAt = now;
                break;

            case OrderStatus.Picked:
                order.PickedAt = now;
                break;

            case OrderStatus.Delivered:
                order.DeliveredAt = now;
                break;
        }

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
    // CANCEL ORDER (USER)
    // =========================
    public async Task CancelOrder(Guid orderId, Guid userId)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
            throw new Exception("Order not found");

        if (order.Status != OrderStatus.Pending &&
            order.Status != OrderStatus.Paid &&
            order.Status != OrderStatus.Preparing)
        {
            throw new Exception("Cannot cancel this order now");
        }

        order.Status = OrderStatus.Cancelled;

        await _context.SaveChangesAsync();
    }

    // =========================
    // USER GET ORDERS
    // =========================
    public async Task<object> GetUserOrders(Guid userId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .Select(o => new
            {
                o.Id,
                Store = o.Store.Name,

                Status = o.Status.ToString(),
                PaymentStatus = o.PaymentStatus.ToString(),
                PaymentMethod = o.PaymentMethod.ToString(),

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
        var result = await _context.Stores
            .AsNoTracking()
            .Select(s => new
            {
                StoreId = s.Id,
                StoreName = s.Name,

                // =========================
                // REVENUE (only delivered)
                // =========================
                TotalRevenue = s.Orders
                    .Where(o => o.Status == OrderStatus.Delivered)
                    .Sum(o => (decimal?)o.FinalPrice) ?? 0,

                // =========================
                // COUNTS
                // =========================
                PendingOrders = s.Orders.Count(o => o.Status == OrderStatus.Pending),

                PaidOrders = s.Orders.Count(o => o.Status == OrderStatus.Paid),

                PreparingOrders = s.Orders.Count(o => o.Status == OrderStatus.Preparing),

                ReadyOrders = s.Orders.Count(o => o.Status == OrderStatus.Ready),

                AssignedOrders = s.Orders.Count(o => o.Status == OrderStatus.Assigned),

                PickedOrders = s.Orders.Count(o => o.Status == OrderStatus.Picked),

                DeliveredOrders = s.Orders.Count(o => o.Status == OrderStatus.Delivered),

                CancelledOrders = s.Orders.Count(o => o.Status == OrderStatus.Cancelled),

                // =========================
                // TOTAL
                // =========================
                TotalOrders = s.Orders.Count()
            })
            .ToListAsync();

        return result;
    }
}