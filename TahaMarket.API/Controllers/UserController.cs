using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services;

[Authorize(Roles = "Customer")]
[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly UserService _service;

    public UserController(UserService service)
    {
        _service = service;
    }

    // =========================================================
    // USER → Get Profile
    // =========================================================
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.GetProfile(userId);

        return Ok(new
        {
            message = "User profile retrieved",
            data = result
        });
    }

    // =========================================================
    // USER → Update Profile
    // =========================================================
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        await _service.UpdateProfile(userId, request);

        return Ok(new
        {
            message = "Profile updated successfully"
        });
    }

    // =========================
    // LOGOUT
    // =========================

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _service.Logout();
        return Ok(new { message = "Logged out successfully" });
    }
}