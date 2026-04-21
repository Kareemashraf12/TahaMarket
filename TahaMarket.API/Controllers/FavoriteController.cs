using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TahaMarket.Application.DTOs;
using TahaMarket.Domain.Enums;

[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoriteController : ControllerBase
{
    private readonly FavoriteService _service;

    public FavoriteController(FavoriteService service)
    {
        _service = service;
    }

    // =========================
    // ADD FAVORITE
    // =========================
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] FavoriteRequest request)
    {
        try
        {
            var result = await _service.Add(request.TargetId, request.Type);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // =========================
    // REMOVE FAVORITE
    // =========================
    [HttpDelete]
    public async Task<IActionResult> Remove([FromBody] FavoriteRequest request)
    {
        try
        {
            var result = await _service.Remove(request.TargetId, request.Type);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // =========================
    // GET MY FAVORITES
    // =========================
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var result = await _service.GetMyFavorites();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // =========================
    // TOGGLE FAVORITE (🔥 BONUS)
    // =========================
    [HttpPost("toggle")]
    public async Task<IActionResult> Toggle([FromBody] FavoriteRequest request)
    {
        try
        {
           
            try
            {
                var removed = await _service.Remove(request.TargetId, request.Type);
                return Ok(new { message = "Removed from favorites" });
            }
            catch
            {
                var added = await _service.Add(request.TargetId, request.Type);
                return Ok(new { message = "Added to favorites" });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}