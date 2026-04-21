using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TahaMarket.Application.DTOs;

[ApiController]
[Route("api/offers")]
public class OfferController : ControllerBase
{
    private readonly OfferService _service;

    public OfferController(OfferService service)
    {
        _service = service;
    }

    // =========================
    // CREATE OFFER (ADMIN ONLY)
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateOfferRequest request)
    {
        var result = await _service.Create(request);

        return Ok(new
        {
            message = "Offer created successfully",
            data = result
        });
    }

    // =========================
    // GET ACTIVE OFFERS (PUBLIC)
    // =========================
    [AllowAnonymous]
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _service.GetActiveOffers();
        return Ok(result);
    }

    // =========================
    // GET BY ID
    // =========================
    [AllowAnonymous]
    [HttpGet("GetById/{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetById(id);
        return Ok(result);
    }

    // =========================
    // UPDATE (ADMIN)
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, CreateOfferRequest request)
    {
        var result = await _service.Update(id, request);

        return Ok(new
        {
            message = "Offer updated successfully",
            data = result
        });
    }

    // =========================
    // DELETE (ADMIN)
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.Delete(id);

        return Ok(new
        {
            message = "Offer deleted successfully"
        });
    }
}