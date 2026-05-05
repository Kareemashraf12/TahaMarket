using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services;

[Authorize(Roles = "Customer")]
[ApiController]
[Route("api/cart")]

public class CartController : ControllerBase
{
    private readonly CartService _cartService;
    private readonly OrderService _orderService;

    public CartController(CartService cartService, OrderService orderService)
    {
        _cartService = cartService;
        _orderService = orderService;
    }

    // =========================
    // GET USER ID
    // =========================
    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (claim == null)
            throw new Exception("Unauthorized");

        return Guid.Parse(claim);
    }

    // =========================
    // ADD TO CART
    // =========================
    [HttpPost("add")]
    public async Task<IActionResult> Add([FromBody] AddToCartRequest request)
    {
        var userId = GetUserId();

        await _cartService.AddToCart(userId, request.StoreId, request.Items);

        return Ok(new { message = "Items added to cart" });
    }

    // =========================
    // UPDATE ITEM
    // =========================
    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] UpdateCartItemRequest request)
    {
        var userId = GetUserId();

        await _cartService.UpdateQuantity(userId, request);

        return Ok(new { message = "Updated" });
    }

    // =========================
    // REMOVE ITEM
    // =========================
    [HttpDelete("remove/{id}")]
    public async Task<IActionResult> Remove(Guid id)
    {
        var userId = GetUserId();

        await _cartService.RemoveItem(userId, id);

        return Ok(new { message = "Removed" });
    }

    // =========================
    // GET CART (PREVIEW)
    // =========================
    [HttpGet("GetCartStore/{storeId}")]
    public async Task<IActionResult> GetCart(Guid storeId)
    {
        var userId = GetUserId();

        var result = await _cartService.GetCart(userId, storeId);

        return Ok(result);
    }

    // =========================
    // CLEAR CART
    // =========================
    [HttpDelete("clear/{cartId}")]
    public async Task<IActionResult> Clear(Guid cartId)
    {
        var userId = GetUserId();

        await _cartService.ClearCart(userId, cartId);

        return Ok(new { message = "Cart cleared" });
    }


    // =========================
    // GET USER CARTS
    // =========================
    [HttpGet("my-cart")]
    public async Task<IActionResult> GetMyCarts()
    {
        var userId = GetUserId();

        var result = await _cartService.GetUserCart(userId);

        return Ok(result);
    }


    // =========================
    // CART PREVIEW (FOR CHECKOUT PAGE)
    // =========================
    [HttpGet("preview/{cartId}")]
    public async Task<IActionResult> Preview(Guid cartId)
    {
        var userId = GetUserId();

        var result = await _cartService.GetCartPreview(userId, cartId);

        return Ok(result);
    }

   
}