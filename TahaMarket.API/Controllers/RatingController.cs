using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services;
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

    // =========================================================
    // CUSTOMER → Rate Store
    // =========================================================
    [Authorize(Roles = "Customer")]
    [HttpPost("store/{storeId}")]
    public async Task<IActionResult> RateStore(
        Guid storeId,
        [FromBody] CreateRatingRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        await _service.RateStore(storeId, userId, request);

        return Ok(new
        {
            message = "Store rated successfully"
        });
    }

    // =========================================================
    // CUSTOMER → Rate Product
    // =========================================================
    [Authorize(Roles = "Customer")]
    [HttpPost("product/{productId}")]
    public async Task<IActionResult> RateProduct(
        Guid productId,
        [FromBody] CreateRatingRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        await _service.RateProduct(productId, userId, request);

        return Ok(new
        {
            message = "Product rated successfully"
        });
    }

    // =========================================================
    // PUBLIC → Get Store Rating
    // =========================================================
    [AllowAnonymous]
    [HttpGet("public/store/{storeId}")]
    public async Task<IActionResult> GetStoreRating(Guid storeId)
    {
        var result = await _service.GetStoreRating(storeId);

        return Ok(new
        {
            message = "Store rating retrieved",
            data = new
            {
                storeId,
                rating = result
            }
        });
    }

    // =========================================================
    // PUBLIC → Get Product Rating
    // =========================================================
    [AllowAnonymous]
    [HttpGet("public/product/{productId}")]
    public async Task<IActionResult> GetProductRating(Guid productId)
    {
        var result = await _service.GetProductRating(productId);

        return Ok(new
        {
            message = "Product rating retrieved",
            data = new
            {
                productId,
                rating = result
            }
        });
    }
}