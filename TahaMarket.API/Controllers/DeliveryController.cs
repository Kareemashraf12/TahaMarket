using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TahaMarket.Application.DTOs;
using TahaMarket.Domain.Entities;

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
    // CREATE DELIVERY (ADMIN ONLY)
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateDeliveryRequest request)
    {
        var result = await _service.Create(request);
        return Ok(result);
    }

    // =========================
    // ASSIGN ORDER (ADMIN / STORE)
    // =========================
    [Authorize(Roles = "Admin,Store")]
    [HttpPost("assign")]
    public async Task<IActionResult> Assign(Guid orderId, Guid deliveryId)
    {
        await _service.AssignOrder(orderId, deliveryId);
        return Ok("Assigned successfully");
    }

    // =========================
    // ACCEPT ORDER (DELIVERY)
    // =========================
    [Authorize(Roles = "Delivery")]
    [HttpPost("accept/{orderId}")]
    public async Task<IActionResult> Accept(Guid orderId)
    {
        await _service.AcceptOrder(orderId);

        return Ok(new
        {
            message = "Order picked successfully"
        });
    }

    // =========================
    // DELIVER ORDER (DELIVERY)
    // =========================
    [Authorize(Roles = "Delivery")]
    [HttpPost("deliver/{orderId}")]
    public async Task<IActionResult> Deliver(Guid orderId)
    {
        await _service.DeliverOrder(orderId);

        return Ok(new
        {
            message = "Order delivered successfully"
        });
    }

    // =========================
    // GET MY ORDERS (DELIVERY)
    // =========================
    [Authorize(Roles = "Delivery")]
    [HttpGet("my-orders")]
    public async Task<IActionResult> MyOrders()
    {
        var result = await _service.GetMyOrders();
        return Ok(result);
    }

    // =========================
    // GET PROFILE (DELIVERY)
    // =========================
    [Authorize(Roles = "Delivery")]
    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var result = await _service.GetProfile();
        return Ok(result);
    }

    // =========================
    // UPDATE PROFILE (DELIVERY)
    // =========================
    [Authorize(Roles = "Delivery")]
    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateDeliveryProfileRequest request)
    {
        var result = await _service.UpdateProfile(request);
        return Ok(result);
    }

    // =========================
    // LOGOUT (DELIVERY)
    // =========================
    [Authorize(Roles = "Delivery")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _service.Logout();

        return Ok(new
        {
            message = "Logged out successfully"
        });
    }
}