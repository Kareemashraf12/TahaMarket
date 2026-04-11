using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TahaMarket.Application.DTOs;

[ApiController]
[Route("api/delivery")]
public class DeliveryController : ControllerBase
{
    private readonly DeliveryService _service;

    public DeliveryController(DeliveryService service)
    {
        _service = service;
    }

    // =========================
    // CREATE DELIVERY (Admin)
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateDeliveryRequest request)
    {
        var result = await _service.Create(request);
        return Ok(result);
    }

    // =========================
    // ASSIGN ORDER
    // =========================
    [Authorize(Roles = "Store,Admin")]
    [HttpPost("assign")]
    public async Task<IActionResult> Assign(AssignOrderRequest request)
    {
        await _service.AssignOrder(request.OrderId, request.DeliveryId);
        return Ok(new { message = "Assigned successfully" });
    }

    // =========================
    // ACCEPT ORDER
    // =========================
    [Authorize(Roles = "Delivery")]
    [HttpPost("accept/{orderId}")]
    public async Task<IActionResult> Accept(Guid orderId)
    {
        await _service.AcceptOrder(orderId);
        return Ok(new { message = "Order picked" });
    }

    // =========================
    // DELIVER ORDER
    // =========================
    [Authorize(Roles = "Delivery")]
    [HttpPost("deliver/{orderId}")]
    public async Task<IActionResult> Deliver(Guid orderId)
    {
        await _service.DeliverOrder(orderId);
        return Ok(new { message = "Delivered successfully" });
    }

    // =========================
    // MY ORDERS
    // =========================
    [Authorize(Roles = "Delivery")]
    [HttpGet("my-orders")]
    public async Task<IActionResult> MyOrders()
    {
        var result = await _service.GetMyOrders();
        return Ok(result);
    }

    // =========================
    // PROFILE
    // =========================
    [Authorize(Roles = "Delivery")]
    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var result = await _service.GetProfile();
        return Ok(result);
    }

    // =========================
    // UPDATE PROFILE
    // =========================
    [Authorize(Roles = "Delivery")]
    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateDeliveryProfileRequest request)
    {
        var result = await _service.UpdateProfile(request);
        return Ok(result);
    }

    // =========================
    // LOGOUT
    // =========================
    [Authorize(Roles = "Delivery")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _service.Logout();
        return Ok(new { message = "Logged out successfully" });
    }
} 