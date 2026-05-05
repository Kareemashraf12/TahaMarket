using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services;

[ApiController]
[Route("api/external-delivery")]
public class ExternalDeliveryController : ControllerBase
{
    private readonly ExternalDeliveryService _service;

    public ExternalDeliveryController(ExternalDeliveryService service)
    {
        _service = service;
    }

    // =========================
    // STORE Order External Delivery
    // =========================
    [Authorize(Roles = "Store")]
    [HttpPost]
    public async Task<IActionResult> Request([FromBody] CreateExternalDeliveryRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized();

        var storeId = Guid.Parse(userIdClaim.Value);

        var result = await _service.RequestDelivery(storeId, request.Address);

        return Ok(result);
    }

    // =========================
    // ADMIN can view all external delivery requests
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _service.GetRequests();
        return Ok(result);
    }
}