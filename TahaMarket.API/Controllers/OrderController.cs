using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TahaMarket.Application.DTOs;
using TahaMarket.Domain.Entities;
using TahaMarket.Domain.Enums;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly OrderService _service;

    public OrderController(OrderService service)
    {
        _service = service;
    }

    // =========================
    // CREATE ORDER FROM CART (CHECKOUT)
    // =========================
    [Authorize(Roles = "Customer")]
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.CreateOrderFromCart(userId, request);

        return Ok(result);
    }

    // =========================
    // USER ORDERS
    // =========================
    [Authorize(Roles = "Customer")]
    [HttpGet("my-orders")]
    public async Task<IActionResult> MyOrders()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.GetUserOrders(userId);

        return Ok(result);
    }

    // =========================
    // CANCEL ORDER (USER)
    // =========================
    [Authorize(Roles = "Customer")]
    [HttpPost("cancel/{orderId}")]
    public async Task<IActionResult> Cancel(Guid orderId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        await _service.CancelOrder(orderId, userId);

        return Ok(new { message = "Order cancelled successfully" });
    }

    // =========================
    // STORE ORDERS
    // =========================
    [Authorize(Roles = "Store")]
    [HttpGet("store-orders")]
    public async Task<IActionResult> StoreOrders([FromQuery] OrderStatus? status)
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.GetStoreOrders(storeId, status);

        return Ok(result);
    }

    // =========================
    // UPDATE ORDER STATUS (STORE)
    // =========================
    [Authorize(Roles = "Store")]
    [HttpPut("status/{orderId}")]
    public async Task<IActionResult> UpdateStatus(Guid orderId, [FromQuery] OrderStatus newStatus)
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        await _service.UpdateOrderStatus(orderId, storeId, newStatus);

        return Ok(new { message = "Order status updated successfully" });
    }

    // =========================
    // STORE DASHBOARD
    // =========================
    [Authorize(Roles = "Store")]
    [HttpGet("store-dashboard")]
    public async Task<IActionResult> StoreDashboard()
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.GetStoreDashboard(storeId);

        return Ok(result);
    }

    // =========================
    // ADMIN GET ALL ORDERS
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpGet("AllForAdmin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllOrders();
        return Ok(result);
    }

    // =========================
    // ADMIN GET ORDERS BY STORE
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpGet("by-store/{storeId}")]
    public async Task<IActionResult> GetByStore(Guid storeId)
    {
        var result = await _service.GetOrdersByStore(storeId);
        return Ok(result);
    }

    // =========================
    // ADMIN DASHBOARD
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpGet("admin-dashboard")]
    public async Task<IActionResult> AdminDashboard()
    {
        var result = await _service.GetAdminDashboard();
        return Ok(result);
    }
}