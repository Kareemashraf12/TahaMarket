using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TahaMarket.Application.DTOs;
using TahaMarket.Domain.Entities;

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
    // USER CREATE ORDER
    // =========================
    [Authorize(Roles = "Customer")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var result = await _service.CreateOrder(userId, request);
        return Ok(result);
    }

    // =========================
    // USER GET MY ORDERS
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
    // STORE GET ORDERS
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
    // STORE ACCEPT ORDER
    // =========================
    [Authorize(Roles = "Store")]
    [HttpPost("accept/{orderId}")]
    public async Task<IActionResult> Accept(Guid orderId)
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        await _service.AcceptOrder(orderId, storeId);
        return Ok(new { message = "Order accepted" });
    }

    // =========================
    // STORE REJECT ORDER
    // =========================
    [Authorize(Roles = "Store")]
    [HttpPost("reject/{orderId}")]
    public async Task<IActionResult> Reject(Guid orderId)
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        await _service.RejectOrder(orderId, storeId);
        return Ok(new { message = "Order rejected" });
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
    [HttpGet("all")]
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