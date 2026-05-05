using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TahaMarket.Application.DTOs;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    // =========================
    //  REGISTER (Send OTP)
    // =========================
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _auth.Register(request);

        if (result is string) // error message
            return BadRequest(new
            {
                message = result
            });

        return Ok(new
        {
            message = "OTP sent successfully",
            data = result
        });
    }

    // =========================
    //  RESEND OTP (REGISTER)
    // =========================
    [HttpPost("resend-register-otp")]
    public async Task<IActionResult> ResendRegisterOtp([FromBody] SendOtpRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _auth.ResendRegisterOtp(request.PhoneNumber);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    // =========================
    //  VERIFY OTP (REGISTER)
    // =========================
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _auth.VerifyOtp(request);

        if (result == null)
        {
            return BadRequest(new
            {
                message = "Invalid or expired OTP"
            });
        }

        return Ok(new
        {
            message = "Account verified successfully",
            data = result
        });
    }

    // =========================
    //  LOGIN
    // =========================
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _auth.Login(request);

        if (result == null)
            return BadRequest(new { message = "Something went wrong" });

        var successProp = result.GetType().GetProperty("success");

        if (successProp != null)
        {
            var success = (bool)successProp.GetValue(result);

            if (!success)
                return BadRequest(result);
        }

        return Ok(result);
    }

    // =========================
    //  SEND RESET OTP
    // =========================
    [HttpPost("send-reset-otp")]
    public async Task<IActionResult> SendResetOtp([FromBody] SendOtpRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _auth.SendResetOtp(request.PhoneNumber);

        if (!result.Success)
        {
            return BadRequest(new
            {
                message = result.Message
            });
        }

        return Ok(new
        {
            message = "OTP sent successfully",
            code = result.Code
        });
    }

    // =========================
    // VERIFY RESET OTP
    // =========================
    [HttpPost("verify-reset-otp")]
    public async Task<IActionResult> VerifyResetOtp([FromBody] VerifyOtpRequest verifyOtpRequest)
    {
        var result = await _auth.VerifyResetOtp(
        verifyOtpRequest.PhoneNumber, verifyOtpRequest.Otp);
        if (!result)
            return BadRequest("Invalid OTP");

        return Ok(new { message = "OTP verified" });
    }


    // =========================
    // RESET PASSWORD
    // =========================
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _auth.ResetPassword(request);

        if (!result)
            return BadRequest("Not allowed or expired");

        return Ok(new { message = "Password reset successfully" });
    }


    // =========================
    // CHANGE PASSWORD
    // =========================
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        await _auth.ChangePassword(userId, request);

        return Ok(new { message = "Password changed successfully" });
    }

    // =========================
    // LOGOUT
    // =========================
    //[Authorize]
    //[HttpPost("logout")]
    //public async Task<IActionResult> Logout()
    //{
    //    var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

    //    await _auth.Logout(userId);

    //    return Ok(new { message = "Logged out successfully" });
    //}



    // =========================
    //   REFRESH TOKEN
    // =========================
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest(new
            {
                message = "Refresh token is required"
            });
        }

        var result = await _auth.RefreshToken(request.RefreshToken);

        if (result == null)
        {
            return Unauthorized(new
            {
                message = "Invalid or expired refresh token"
            });
        }

        return Ok(new
        {
            message = "Token refreshed successfully",
            data = result
        });
    }
}