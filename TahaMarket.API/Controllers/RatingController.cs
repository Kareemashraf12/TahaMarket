using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TahaMarket.Application.DTOs;
using TahaMarket.Domain.Enums;

[ApiController]
[Route("api/ratings")]
public class RatingController : ControllerBase
{
    private readonly RatingService _service;

    public RatingController(RatingService service)
    {
        _service = service;
    }

    // =========================
    // ADD / UPDATE
    // =========================
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Add(AddRatingRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        await _service.AddOrUpdate(userId, request);

        return Ok(new { message = "Rating saved successfully" });
    }

    // =========================
    // SUMMARY
    // =========================
    [AllowAnonymous]
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(Guid targetId, RatingTargetType type)
    {
        var result = await _service.GetSummary(targetId, type);
        return Ok(result);
    }

    // =========================
    // COMMENTS
    // =========================
    [AllowAnonymous]
    [HttpGet("comments")]
    public async Task<IActionResult> GetComments(Guid targetId, RatingTargetType type)
    {
        var result = await _service.GetComments(targetId, type);
        return Ok(result);
    }

    // =========================
    //  FULL DETAILS 
    // =========================
    [AllowAnonymous]
    [HttpGet("details")]
    public async Task<IActionResult> GetFullDetails(Guid targetId, RatingTargetType type)
    {
        var result = await _service.GetFullDetails(targetId, type);
        return Ok(result);
    }

    // =========================
    // DELETE MY RATING
    // =========================
    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> Delete(Guid targetId, RatingTargetType type)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        await _service.Delete(userId, targetId, type);

        return Ok(new { message = "Rating deleted successfully" });
    }
}