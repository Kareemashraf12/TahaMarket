using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services.Common;
using TahaMarket.Domain.Entities;
using TahaMarket.Domain.Enums;
using TahaMarket.Infrastructure.Data;

namespace TahaMarket.Application.Services
{
    public class CartService
    {
        private readonly ApplicationDbContext _context;
        private readonly DeliveryPricingService _pricing;
        private readonly DistanceService _distance;
        private readonly OfferService _offerService;
        private readonly FileUrlService _fileUrlService;

        public CartService(ApplicationDbContext context, DeliveryPricingService pricing ,DistanceService distance ,OfferService offerService , FileUrlService fileUrlService)
        {
            _context = context;
            _pricing = pricing;
            _distance = distance;
            _offerService = offerService;
            _fileUrlService = fileUrlService;
        }

        // =========================
        // GET OR CREATE CART
        // =========================
        private async Task<Cart> GetOrCreateCart(Guid userId, Guid storeId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.AddOns)
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.StoreId == storeId);

            if (cart != null)
            {
                // ensure fresh data (no stale tracking)
                await _context.Entry(cart)
                    .Collection(c => c.Items)
                    .Query()
                    .Include(i => i.AddOns)
                    .LoadAsync();

                return cart;
            }

            cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StoreId = storeId,
                UpdatedAt = DateTime.UtcNow,
                Items = new List<CartItem>()
            };

            _context.Carts.Add(cart);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var fallbackCart = await _context.Carts
                    .Include(c => c.Items)
                        .ThenInclude(i => i.AddOns)
                    .FirstOrDefaultAsync(c =>
                        c.UserId == userId &&
                        c.StoreId == storeId);

                if (fallbackCart != null)
                    return fallbackCart;

                throw;
            }

            return cart;
        }

        // =========================================
        // HELPER: CHECK IF ADD-ONS ARE THE SAME
        // =========================================
        private bool IsSameAddOns(CartItem item, List<Guid>? addOnIds)
        {
            var existingIds = (item.AddOns ?? new List<CartItemAddOn>())
                .Select(a => a.AddOnOptionId)
                .OrderBy(x => x)
                .ToList();

            var newIds = (addOnIds ?? new List<Guid>())
                .OrderBy(x => x)
                .ToList();

            return existingIds.SequenceEqual(newIds);
        }


        // =========================
        // ADD TO CART
        // =========================
        public async Task AddToCart(Guid userId, Guid storeId, List<AddToCartItemDto> requests)
        {
            if (requests == null || !requests.Any())
                throw new Exception("Cart items cannot be empty");

            // =========================
            // VALIDATE PRODUCTS
            // =========================
            var productIds = requests.Select(x => x.ProductId).Distinct().ToList();

            var products = await _context.Products
                .Include(p => p.Variants)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            if (products.Count != productIds.Count)
                throw new Exception("One or more products not found");

            // =========================
            // VALIDATE STORE
            // =========================
            if (products.Any(p => p.StoreId != storeId))
                throw new Exception("All products must belong to the same store");

            // =========================
            // GET CART
            // =========================
            var cart = await GetOrCreateCart(userId, storeId);

            // reload with tracking clean state
            await _context.Entry(cart)
                .Collection(c => c.Items)
                .Query()
                .Include(i => i.AddOns)
                .LoadAsync();

            // =========================
            // PROCESS ITEMS
            // =========================
            foreach (var request in requests)
            {
                var product = products.First(p => p.Id == request.ProductId);

                var variant = product.Variants
                    .FirstOrDefault(v => v.Id == request.VariantId);

                if (variant == null)
                    throw new Exception($"Invalid variant for product {product.Name}");

                // =========================
                // GET ADD-ONS
                // =========================
                List<AddOnOption> selectedOptions = new();

                if (request.AddOnOptionIds != null && request.AddOnOptionIds.Any())
                {
                    selectedOptions = await _context.AddOnOptions
                        .Include(o => o.AddOnGroup)
                        .Where(o =>
                            request.AddOnOptionIds.Contains(o.Id) &&
                            o.IsActive)
                        .ToListAsync();

                    if (selectedOptions.Count != request.AddOnOptionIds.Count)
                        throw new Exception("Invalid add-on options");

                    var valid = selectedOptions.All(o =>
                        o.AddOnGroup.ProductId == request.ProductId ||
                        o.AddOnGroup.StoreId == storeId);

                    if (!valid)
                        throw new Exception("Add-ons not valid for this product/store");
                }

                // =========================
                // MERGE LOGIC
                // =========================
                var existingItem = cart.Items
                    .FirstOrDefault(i =>
                        i.ProductId == request.ProductId &&
                        i.VariantId == request.VariantId &&
                        IsSameAddOns(i, request.AddOnOptionIds));

                if (existingItem != null)
                {
                    existingItem.Quantity += request.Quantity;
                }
                else
                {
                    // =========================
                    // CREATE ITEM (SEPARATE INSERT)
                    // =========================
                    var newItem = new CartItem
                    {
                        Id = Guid.NewGuid(),
                        CartId = cart.Id, // 🔥 critical FK
                        ProductId = request.ProductId,
                        VariantId = request.VariantId,
                        Quantity = request.Quantity,
                        Note = request.Note ?? string.Empty
                    };

                    await _context.CartItems.AddAsync(newItem);

                    // save first to avoid concurrency issue
                    await _context.SaveChangesAsync();

                    // =========================
                    // ADD ADD-ONS AFTER ITEM EXISTS
                    // =========================
                    if (selectedOptions.Any())
                    {
                        var addOns = selectedOptions.Select(o => new CartItemAddOn
                        {
                            Id = Guid.NewGuid(),
                            CartItemId = newItem.Id, 
                            AddOnOptionId = o.Id,
                            Name = o.Name,
                            Price = o.Price
                        }).ToList();

                        await _context.Set<CartItemAddOn>().AddRangeAsync(addOns);
                    }
                }
            }

            // =========================
            // UPDATE CART
            // =========================
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // =========================
        // UPDATE QUANTITY
        // =========================
        public async Task UpdateQuantity(Guid userId, UpdateCartItemRequest request)
        {
            var item = await _context.CartItems
                .FirstOrDefaultAsync(i =>
                    i.Id == request.CartItemId &&
                    i.Cart.UserId == userId);

            if (item == null)
                throw new Exception("Item not found");

            // =========================
            // UPDATE OR REMOVE
            // =========================
            if (request.Quantity <= 0)
            {
                _context.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = request.Quantity;
            }

            // =========================
            // ONLY UPDATE CART TIMESTAMP
            // (NO NAVIGATION TRACKING)
            // =========================
            await _context.Carts
                .Where(c => c.Id == item.CartId)
                .ExecuteUpdateAsync(c => c
                    .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));

            await _context.SaveChangesAsync();
        }

        // =========================
        // REMOVE ITEM
        // =========================
        public async Task RemoveItem(Guid userId, Guid cartItemId)
        {
            var item = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i =>
                    i.Id == cartItemId &&
                    i.Cart.UserId == userId);

            if (item == null)
                throw new Exception("Item not found");

            // remove item
            _context.CartItems.Remove(item);

            // safer update (avoid navigation side effects)
            await _context.Carts
                .Where(c => c.Id == item.CartId)
                .ExecuteUpdateAsync(c => c
                    .SetProperty(x => x.UpdatedAt, DateTime.UtcNow));

            await _context.SaveChangesAsync();
        }

        // =========================
        // GET CART (PREVIEW)
        // =========================
        public async Task<CartResponse> GetCart(Guid userId, Guid storeId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Variants)
                .Include(c => c.Items)
                    .ThenInclude(i => i.AddOns)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.StoreId == storeId);

            if (cart == null)
                return new CartResponse
                {
                    CartId = Guid.Empty,
                    StoreId = storeId,
                    Items = new List<CartItemResponse>(),
                    SubTotal = 0,
                    DeliveryFee = 0,
                    Total = 0
                };

            decimal subTotal = 0;

            var items = new List<CartItemResponse>();

            foreach (var item in cart.Items)
            {
                var variant = item.Product.Variants
                    .First(v => v.Id == item.VariantId);

                var addOnsTotal = item.AddOns.Sum(a => a.Price);

                var price = variant.Price + addOnsTotal;

                subTotal += price * item.Quantity;

                items.Add(new CartItemResponse
                {
                    CartItemId = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    VariantId = item.VariantId,
                    Quantity = item.Quantity,
                    Price = price
                });
            }

            var deliveryFee = 0;

            return new CartResponse
            {
                CartId = cart.Id,
                StoreId = storeId,
                Items = items,
                SubTotal = subTotal,
                DeliveryFee = deliveryFee,
                Total = subTotal + deliveryFee
            };
        }

        // =========================
        // CLEAR CART
        // =========================
        public async Task ClearCart(Guid userId, Guid cartId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.AddOns)
                .FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == userId);

            if (cart == null)
                throw new Exception("Cart not found");

            // =========================
            // DELETE ADD-ONS FIRST (SAFE)
            // =========================
            var addOns = cart.Items.SelectMany(i => i.AddOns).ToList();
            _context.Set<CartItemAddOn>().RemoveRange(addOns);

            // =========================
            // DELETE ITEMS
            // =========================
            _context.CartItems.RemoveRange(cart.Items);

            // =========================
            // UPDATE CART META
            // =========================
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

        }
        // ==========================
        // GET USER CARTS (FOR DASHBOARD)
        // =========================
        public async Task<object> GetUserCart(Guid userId)
        {
            var cart = await _context.Carts
                .AsNoTracking()
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Variant)
                .Include(c => c.Items)
                    .ThenInclude(i => i.AddOns)
                .Include(c => c.Store)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.UpdatedAt) // 🔥 latest cart
                .FirstOrDefaultAsync();

            if (cart == null)
                return null;

            var result = new
            {
                CartId = cart.Id,
                StoreId = cart.StoreId,
                StoreName = cart.Store.Name,

                Items = cart.Items.Select(i => new
                {
                    CartItemId = i.Id,
                    i.ProductId,
                    ProductName = i.Product.Name,
                    ImageUrl = _fileUrlService.GetFullUrl(i.Product.ImageUrl),

                    i.VariantId,
                    VariantName = i.Variant.Name,
                    i.Quantity,

                    BasePrice = i.Variant.Price,

                    AddOns = i.AddOns.Select(a => new
                    {
                        Id = a.AddOnOptionId,
                        Name = a.Name,
                        Price = a.Price
                    }).ToList(),

                    AddOnsTotal = i.AddOns.Sum(a => a.Price),

                    Total = i.Quantity * (i.Variant.Price + i.AddOns.Sum(a => a.Price))
                }).ToList(),

                TotalPrice = cart.Items.Sum(i =>
                    i.Quantity * (i.Variant.Price + i.AddOns.Sum(a => a.Price))
                ),

                ItemsCount = cart.Items.Count
            };

            return result;
        }


        // ======================================
        // GET CART PREVIEW (FOR CHECKOUT)
        // ======================================
        public async Task<CartPreviewDto> GetCartPreview(Guid userId, Guid cartId)
        {
            var cart = await _context.Carts
                .AsNoTracking()
                .Include(c => c.Store)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Variant)
                .Include(c => c.Items)
                    .ThenInclude(i => i.AddOns)
                .FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == userId);

            if (cart == null || !cart.Items.Any())
                throw new Exception("Cart is empty");

            var address = await _context.UserAddresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);

            if (address == null)
                throw new Exception("No default address found");

            decimal subtotal = 0;

            var items = new List<CartItemPreviewDto>();

            foreach (var item in cart.Items)
            {
                var product = item.Product;
                var variant = item.Variant;

                var addOnsTotal = item.AddOns.Sum(a => a.Price);

                var addOnsList = item.AddOns.Select(a => new CartItemAddOnPreviewDto
                {
                    AddOnOptionId = a.AddOnOptionId,
                    Name = a.Name,
                    Price = a.Price
                }).ToList();

                var discount = await _offerService.CalculateDiscount(
                    product.Id,
                    product.CategoryId,
                    variant.Price
                );

                var finalUnitPrice = (variant.Price - discount) + addOnsTotal;

                subtotal += finalUnitPrice * item.Quantity;

                items.Add(new CartItemPreviewDto
                {
                    ProductName = product.Name,
                    VariantName = variant.Name,
                    ImageUrl = _fileUrlService.GetFullUrl(product.ImageUrl),
                    Quantity = item.Quantity,
                    DiscountedPrice = discount,
                    Price = finalUnitPrice,
                    Total = finalUnitPrice * item.Quantity,
                    Note = item.Note,
                    AddOns = addOnsList
                });
            }

            var delivery = await _pricing.CalculateDeliveryCost(
                cart.Store.Latitude,
                cart.Store.Longitude,
                address.Latitude,
                address.Longitude
            );

            var deliveryFee = delivery.TotalDeliveryCost;

            var finalPriceTotal = subtotal + deliveryFee;

            return new CartPreviewDto
            {
                CartId = cart.Id,
                StoreName = cart.Store.Name,
                Items = items,
                TotalPrice = subtotal,
                DeliveryFee = deliveryFee,
                FinalPrice = finalPriceTotal
            };
        }


    }
}
