using Microsoft.AspNetCore.Mvc;
using TahaMarket.Application.DTOs;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _auth.Register(request);

        if (result == null)
            return BadRequest(new
            {
                message = "Phone number already exists"
            });

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _auth.Login(request);

        if (result == null)
        {
            return Unauthorized(new
            {
                message = "Invalid phone number or password"
            });
        }

        return Ok(result);
    }
        
    
}