using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services.Common;

[ApiController]
[Route("api/delivery-pricing")]
public class DeliveryPricingController : ControllerBase
{
    private readonly DeliveryPricingService _service;

    public DeliveryPricingController(DeliveryPricingService service)
    {
        _service = service;
    }

    // =========================
    // GET CURRENT PRICING
    // =========================
    [HttpGet]
    public async Task<IActionResult> GetCurrentPricing()
    {
        var result = await _service.GetCurrentPricing();
        return Ok(result);
    }

    // =========================
    // UPDATE PRICING (ADMIN)
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpPost("update")]
    public async Task<IActionResult> UpdatePricing([FromBody] UpdateDeliveryPricingRequest request)
    {
        var result = await _service.UpdatePricing(request);

        return Ok(new
        {
            message = "Delivery pricing updated successfully",
            data = result
        });
    }
}