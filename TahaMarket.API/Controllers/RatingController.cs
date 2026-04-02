using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TahaMarket.Domain.Entities;

[ApiController]
[Route("api/ratings")]
public class RatingController : ControllerBase
{
    private readonly RatingService _service;

    public RatingController(RatingService service)
    {
        _service = service;
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("store/{storeId}")]
    public async Task<IActionResult> RateStore(Guid storeId, CreateRatingRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        await _service.RateStore(storeId, userId, request);
        return Ok();
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("product/{productId}")]
    public async Task<IActionResult> RateProduct(Guid productId, CreateRatingRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        await _service.RateProduct(productId, userId, request);
        return Ok();
    }

    [HttpGet("store/{storeId}")]
    public async Task<IActionResult> GetStoreRating(Guid storeId)
    {
        return Ok(await _service.GetStoreRating(storeId));
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetProductRating(Guid productId)
    {
        return Ok(await _service.GetProductRating(productId));
    }
}