using Microsoft.EntityFrameworkCore;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services.Common;
using TahaMarket.Domain.Entities;
using TahaMarket.Domain.Enums;
using TahaMarket.Infrastructure.Data;

public class OrderService
{
    private readonly ApplicationDbContext _context;
    private readonly DeliveryPricingService _pricing;
    private readonly FileUrlService _fileUrl;
    private readonly OfferService _offerService;

    public OrderService(ApplicationDbContext context, DeliveryPricingService pricing, FileUrlService fileUrl, OfferService offerService)
    {
        _context = context;
        _pricing = pricing;
        _fileUrl = fileUrl;
        _offerService = offerService;
    }

    // =========================
    // CREATE ORDER FROM CART (CHECKOUT)
    // =========================
    public async Task<object> CreateOrderFromCart(Guid userId, CheckoutRequest request)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cart = await _context.Carts
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Product)
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Variant)
                    .Include(c => c.Items)
                        .ThenInclude(i => i.AddOns) // 🔥 مهم
                    .FirstOrDefaultAsync(c => c.Id == request.CartId && c.UserId == userId);

                if (cart == null || cart.Items.Count == 0)
                    throw new Exception("Cart is empty");

                var store = await _context.Stores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == cart.StoreId);

                var address = await _context.UserAddresses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == userId);

                if (store == null || address == null)
                    throw new Exception("Invalid store or address");

                decimal subtotal = 0;

                var order = new Order
                {
                    UserId = userId,
                    StoreId = cart.StoreId,
                    AddressId = request.AddressId,
                    PaymentMethod = request.PaymentMethod,
                    PaymentStatus = PaymentStatus.Pending,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    Items = new List<OrderItem>()
                };

                foreach (var item in cart.Items)
                {
                    var variant = item.Variant;

                    // =========================
                    // STOCK VALIDATION
                    // =========================
                    if (variant.IsStockTracked)
                    {
                        if (variant.StockQuantity == null || variant.StockQuantity < item.Quantity)
                            throw new Exception($"Not enough stock for {variant.Name}");

                        variant.StockQuantity -= item.Quantity;
                    }

                    // =========================
                    // BASE PRICE = variant + add-ons
                    // =========================
                    var addOnsTotal = item.AddOns.Sum(a => a.Price);
                    var basePrice = variant.Price + addOnsTotal;

                    // =========================
                    // APPLY DISCOUNT
                    // =========================
                    var discount = await _offerService.CalculateDiscount(
                        item.ProductId,
                        item.Product.CategoryId,
                        basePrice
                    );

                    var finalPrice = basePrice - discount;

                    subtotal += finalPrice * item.Quantity;

                    // =========================
                    // CREATE ORDER ITEM
                    // =========================
                    order.Items.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = finalPrice,
                        Note = item.Note ?? string.Empty,

                        //  snapshot 
                        VariantName = variant.Name,
                        BasePrice = basePrice,
                        Discount = discount,

                        AddOns = item.AddOns.Select(a => new OrderItemAddOn
                        {
                            Name = a.Name,
                            Price = a.Price
                        }).ToList()
                    });
                }

                var delivery = await _pricing.CalculateDeliveryCost(
                    store.Latitude,
                    store.Longitude,
                    address.Latitude,
                    address.Longitude
                );

                order.TotalPrice = subtotal;
                order.DeliveryFee = delivery.TotalDeliveryCost;
                order.FinalPrice = subtotal + delivery.TotalDeliveryCost;

                _context.Orders.Add(order);

                _context.Carts.Remove(cart);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new
                {
                    order.Id,
                    order.Status,
                    order.TotalPrice,
                    order.DeliveryFee,
                    order.FinalPrice
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        });
    }
    // =========================
    // STORE GET ORDERS
    // =========================
    public async Task<object> GetStoreOrders(Guid storeId, OrderStatus? status = null)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Where(o => o.StoreId == storeId)
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Items)
                .ThenInclude(i => i.AddOns)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        return await query.Select(o => new
        {
            o.Id,
            User = o.User != null ? o.User.Name : "",
            o.Status,
            o.TotalPrice,
            o.FinalPrice,

            Items = o.Items.Select(i => new
            {
                Product = i.Product != null ? i.Product.Name : "",
                i.Quantity,
                i.Price,
                i.Note,
                i.VariantName,

                AddOns = i.AddOns.Select(a => new
                {
                    a.Name,
                    a.Price
                })
            }),

            o.CreatedAt
        }).ToListAsync();
    }

    // =========================
    // STORE DASHBOARD (OPTIMIZED)
    // =========================
    public async Task<object> GetStoreDashboard(Guid storeId)
    {
        var orders = _context.Orders
            .AsNoTracking()
            .Where(o => o.StoreId == storeId);

        var list = await orders.ToListAsync();

        return new
        {
            StoreId = storeId,

            TotalOrders = list.Count,
            Pending = list.Count(x => x.Status == OrderStatus.Pending),
            Preparing = list.Count(x => x.Status == OrderStatus.Preparing),
            Ready = list.Count(x => x.Status == OrderStatus.Ready),
            Assigned = list.Count(x => x.Status == OrderStatus.Assigned),
            Picked = list.Count(x => x.Status == OrderStatus.Picked),
            Delivered = list.Count(x => x.Status == OrderStatus.Delivered),
            Cancelled = list.Count(x => x.Status == OrderStatus.Cancelled),

            TotalRevenue = list
                .Where(x => x.Status == OrderStatus.Delivered)
                .Sum(x => x.FinalPrice),

            TodayRevenue = list
                .Where(x => x.Status == OrderStatus.Delivered &&
                            x.CreatedAt.Date == DateTime.UtcNow.Date)
                .Sum(x => x.FinalPrice),

            ActiveOrders = list.Count(x =>
                x.Status != OrderStatus.Delivered &&
                x.Status != OrderStatus.Cancelled)
        };
    }

    // =========================
    // UPDATE ORDER STATUS (STORE)
    // =========================
    public async Task UpdateOrderStatus(Guid orderId, Guid storeId, OrderStatus newStatus)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.StoreId == storeId);

        if (order == null)
            throw new Exception("Order not found");

        if (order.Status is OrderStatus.Delivered or OrderStatus.Cancelled)
            throw new Exception("Order already completed");

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

        order.Status = newStatus;

        var now = DateTime.UtcNow;

        if (newStatus == OrderStatus.Preparing)
            order.AcceptedAt = now;

        if (newStatus == OrderStatus.Ready)
            order.ReadyAt = now;

        if (newStatus == OrderStatus.Assigned)
            order.AssignedAt = now;

        if (newStatus == OrderStatus.Picked)
            order.PickedAt = now;

        if (newStatus == OrderStatus.Delivered)
            order.DeliveredAt = now;

        await _context.SaveChangesAsync();
    }

    // =========================
    // GET ALL ORDERS (ADMIN)
    // =========================
    public async Task<object> GetAllOrders()
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Store)
            .Include(o => o.User)
            .Select(o => new
            {
                o.Id,
                Store = o.Store != null ? o.Store.Name : "",
                User = o.User != null ? o.User.Name : "",
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

        if (order.Status is OrderStatus.Delivered or OrderStatus.Cancelled)
            throw new Exception("Cannot cancel this order");

        order.Status = OrderStatus.Cancelled;

        await _context.SaveChangesAsync();
    }

    // =========================
    // USER ORDERS
    // =========================
    public async Task<object> GetUserOrders(Guid userId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .Include(o => o.Store)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Items)
                .ThenInclude(i => i.AddOns)
            .Select(o => new
            {
                OrderId = o.Id,
                OrderNumber = o.Id,
                o.CreatedAt,
                o.Status,
                o.PaymentStatus,

                StoreId = o.StoreId,
                StoreName = o.Store != null ? o.Store.Name : "",
                StoreImage = _fileUrl.GetFullUrl(o.Store.ImageUrl),

                ItemsCount = o.Items.Count,
                TotalQuantity = o.Items.Sum(i => i.Quantity),

                Items = o.Items.Select(i => new
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product != null ? i.Product.Name : "",
                    Quantity = i.Quantity,
                    Price = i.Price,
                    VariantName = i.VariantName,

                    AddOns = i.AddOns.Select(a => new
                    {
                        a.Name,
                        a.Price
                    })
                }),

                TotalPrice = o.TotalPrice,
                FinalPrice = o.FinalPrice
            })
            .ToListAsync();
    }
    // =========================
    // ADMIN ORDERS BY STORE
    // =========================
    public async Task<object> GetOrdersByStore(Guid storeId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.StoreId == storeId)
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Select(o => new
            {
                o.Id,
                User = o.User != null ? o.User.Name : "",
                o.Status,
                o.TotalPrice,
                o.FinalPrice,
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
            .AsNoTracking()
            .Include(s => s.Orders)
            .ToListAsync();

        return stores.Select(s => new
        {
            s.Id,
            s.Name,

            TotalRevenue = s.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .Sum(o => o.FinalPrice),

            Pending = s.Orders.Count(o => o.Status == OrderStatus.Pending),
            Preparing = s.Orders.Count(o => o.Status == OrderStatus.Preparing),
            Ready = s.Orders.Count(o => o.Status == OrderStatus.Ready),
            Assigned = s.Orders.Count(o => o.Status == OrderStatus.Assigned),
            Picked = s.Orders.Count(o => o.Status == OrderStatus.Picked),
            Delivered = s.Orders.Count(o => o.Status == OrderStatus.Delivered),
            Cancelled = s.Orders.Count(o => o.Status == OrderStatus.Cancelled),

            TotalOrders = s.Orders.Count
        });
    }

    public async Task<Order?> GetOrderById(Guid id)
    {
        return await _context.Orders
            .Include(o => o.Store)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}