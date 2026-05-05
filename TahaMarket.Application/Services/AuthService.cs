using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services.Common;
using TahaMarket.Domain.Entities;
using TahaMarket.Domain.Enums;
using TahaMarket.Infrastructure.Data;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtService _jwt;
    private readonly OtpService _otpService;
    private readonly FileUrlService _fileUrl;
    private readonly IConfiguration _config;
    public AuthService(ApplicationDbContext context, JwtService jwt, OtpService otpService , FileUrlService fileUrl, IConfiguration config)
    {
        _context = context;
        _jwt = jwt;
        _otpService = otpService;
        _fileUrl = fileUrl;
        _config = config;
    }

    // REGISTER
    public async Task<object> Register(RegisterRequest request)
    {
        var phone = request.PhoneNumber.Trim();

        if (await _context.Users.AnyAsync(u => u.PhoneNumber == phone))
            return "Phone already exists";

        var user = new User
        {
            PhoneNumber = phone,
            Name = request.Name,

            UserType = UserType.Customer,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsVerified = false,
            ImageUrl = "/images/users/user.png"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var otp = await _otpService.GenerateOtp(phone);

        return new { message = "OTP sent", code = otp };
    }

    // VERIFY
    public async Task<object?> VerifyOtp(VerifyOtpRequest request)
    {
        var phone = request.PhoneNumber.Trim();

        var valid = await _otpService.VerifyOtp(phone, request.Otp);

        if (!valid)
            return null;

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == phone);

        if (user == null)
            return null;

        user.IsVerified = true;
        await _context.SaveChangesAsync();

        //  generate token 
        var tokens = await GenerateUserTokens(user);

        
        return new
        {
            user = new
            {
                id = user.Id,
                name = user.Name,
                phoneNumber = user.PhoneNumber,
                imageUrl = _fileUrl.GetFullUrl(user.ImageUrl)
            },
            accessToken = tokens.AccessToken,
            refreshToken = tokens.RefreshToken,
            expiration = tokens.Expiration
        };
    }

    // RESEND OTP
    public async Task<OtpResponse> ResendRegisterOtp(string phone)
    {
        var exists = await _context.Users
            .AnyAsync(u => u.PhoneNumber == phone && u.IsVerified);

        if (exists)
        {
            return new OtpResponse
            {
                Success = false,
                Message = "User already verified"
            };
        }

        var code = await _otpService.ResendOtp(phone);

        return new OtpResponse
        {
            Success = true,
            Message = "OTP sent",
            Code = code
        };
    }

    // =========================
    // VERIFY RESET OTP
    // =========================
    public async Task<bool> VerifyResetOtp(string PhoneNumber, string otp)
    {
        var valid = await _otpService.VerifyOtp(PhoneNumber, otp);

        if (!valid) return false;

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == PhoneNumber);

        if (user == null) return false;

        user.CanResetPassword = true;
        user.ResetAllowedUntil = DateTime.UtcNow.AddMinutes(5);

        await _context.SaveChangesAsync();

        return true;
    }


    // LOGIN
    public async Task<object?> Login(LoginRequest request)
    {
        var phone = request.PhoneNumber.Trim();

        // ================= USER =================
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == phone);

        if (user != null)
        {
            // Account is not verified
            if (!user.IsVerified)
            {
                return new
                {
                    success = false,
                    message = "Account is not verified. Please verify OTP first",
                    code = "NOT_VERIFIED"
                };
            }

            // Password check = false
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return new
                {
                    success = false,
                    message = "Invalid phone number or password"
                };
            }

            var tokens = await GenerateUserTokens(user);

            return new
            {
                success = true,
                message = "Login successful",
                type = user.UserType.ToString(),
                data = new
                {
                    user.Id,
                    user.Name,
                    user.PhoneNumber,
                    ImageUrl = _fileUrl.GetFullUrl(user.ImageUrl),
                    accessToken = tokens.AccessToken,
                    refreshToken = tokens.RefreshToken,
                    expiration = tokens.Expiration
                }
            };
        }

        // ================= STORE =================
        var store = await _context.Stores
            .FirstOrDefaultAsync(s => s.PhoneNumber == phone);

        if (store != null)
        {
            if (!BCrypt.Net.BCrypt.Verify(request.Password, store.PasswordHash))
            {
                return new
                {
                    success = false,
                    message = "Invalid phone number or password"
                };
            }

            var tokens = await GenerateStoreTokens(store);

            return new
            {
                success = true,
                message = "Login successful",
                type = "Store",
                data = new
                {
                    store.Id,
                    store.Name,
                    store.PhoneNumber,
                    store.Address,
                    ImageUrl = _fileUrl.GetFullUrl(store.ImageUrl),
                    accessToken = tokens.AccessToken,
                    refreshToken = tokens.RefreshToken,
                    expiration = tokens.Expiration
                }
            };
        }

        // ================= DELIVERY =================
        var delivery = await _context.Deliveries
            .FirstOrDefaultAsync(d => d.PhoneNumber == phone);

        if (delivery != null)
        {
            if (!BCrypt.Net.BCrypt.Verify(request.Password, delivery.PasswordHash))
            {
                return new
                {
                    success = false,
                    message = "Invalid phone number or password"
                };
            }
            delivery.Status = DeliveryStatus.Online;
            var tokens = await GenerateDeliveryTokens(delivery);

            return new
            {
                success = true,
                message = "Login successful",
                type = "Delivery",
                data = new
                {
                    delivery.Id,
                    delivery.Name,
                    delivery.PhoneNumber,
                    delivery.VehicleType,
                    delivery.Status,
                    delivery.CurrentLatitude,
                    delivery.CurrentLongitude,
                    ImageUrl = _fileUrl.GetFullUrl(delivery.ImageUrl),
                    delivery.Balance,
                    accessToken = tokens.AccessToken,
                    refreshToken = tokens.RefreshToken,
                    expiration = tokens.Expiration
                }
            };
        }

        // number not found in any table
        return new
        {
            success = false,
            message = "Invalid phone number or password"
        };
    }



    // SEND REGISTER OTP
    public async Task<OtpResult> SendRegisterOtp(string phone)
    {
        if (await _context.Users.AnyAsync(u => u.PhoneNumber == phone))
            return new OtpResult { Success = false, Message = "Phone exists" };

        var code = await _otpService.GenerateOtp(phone);

        return new OtpResult { Success = true, Code = code };
    }

    // SEND RESET OTP
    public async Task<OtpResult> SendResetOtp(string phone)
    {
        if (!await _context.Users.AnyAsync(u => u.PhoneNumber == phone))
            return new OtpResult { Success = false, Message = "Phone not found" };

        var code = await _otpService.GenerateOtp(phone);

        return new OtpResult { Success = true, Code = code };
    }

    // =========================
    // RESET PASSWORD (NO OTP AGAIN)
    // =========================
    public async Task<bool> ResetPassword(ResetPasswordRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);

        if (user == null) return false;

        if (!user.CanResetPassword || user.ResetAllowedUntil < DateTime.UtcNow)
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        //  invalidate
        user.CanResetPassword = false;
        user.ResetAllowedUntil = null;

        await _context.SaveChangesAsync();

        return true;
    }


    // =========================
    // CHANGE PASSWORD (LOGGED IN)
    // =========================
    public async Task<bool> ChangePassword(Guid userId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            throw new Exception("User not found");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new Exception("Current password is incorrect");

        if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
            throw new Exception("New password must be different");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        await _context.SaveChangesAsync();

        return true;
    }

   

    // =========================
    // REFRESH TOKEN
    // =========================
    public async Task<object?> RefreshToken(string refreshToken)
    {
        // ================= USER =================
        var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiry > DateTime.UtcNow);
        if (user != null)
        {
            var tokens = await GenerateUserTokens(user);
            return new
            {
                type = user.UserType.ToString(),
                user = new
                {
                    user.Id,
                    user.Name,
                    user.PhoneNumber,
                    ImageUrl = _fileUrl.GetFullUrl(user.ImageUrl)
                },
                accessToken = tokens.AccessToken,
                refreshToken = tokens.RefreshToken,
                expiration = tokens.Expiration
            };
        }

        // ================= STORE =================
        var store = await _context.Stores.FirstOrDefaultAsync(s => s.RefreshToken == refreshToken && s.RefreshTokenExpiry > DateTime.UtcNow);
        if (store != null)
        {
            var tokens = await GenerateStoreTokens(store);
            return new
            {
                type = "Store",
                store = new
                {
                    store.Id,
                    store.Name,
                    store.PhoneNumber,
                    store.Address,
                    ImageUrl = _fileUrl.GetFullUrl(store.ImageUrl)
                },
                accessToken = tokens.AccessToken,
                refreshToken = tokens.RefreshToken,
                expiration = tokens.Expiration
            };
        }

        // ================= DELIVERY =================
        var delivery = await _context.Deliveries.FirstOrDefaultAsync(d => d.RefreshToken == refreshToken && d.RefreshTokenExpiry > DateTime.UtcNow);
        if (delivery != null)
        {
            var tokens = await GenerateDeliveryTokens(delivery);
            return new
            {
                type = "Delivery",
                delivery = new
                {
                    delivery.Id,
                    delivery.Name,
                    delivery.PhoneNumber,
                    delivery.VehicleType,
                    delivery.Status,
                    delivery.CurrentLatitude,
                    delivery.CurrentLongitude,
                    ImageUrl = _fileUrl.GetFullUrl(delivery.ImageUrl),
                    delivery.Balance
                },
                accessToken = tokens.AccessToken,
                refreshToken = tokens.RefreshToken,
                expiration = tokens.Expiration
            };
        }

        return null;
    }



    // ======================= TOKENS ==========================

    private async Task<AuthResponse> GenerateUserTokens(User user)
    {
        var accessToken = _jwt.GenerateToken(
            user.Id.ToString(),
            user.UserType.ToString(),
            user.PhoneNumber
        );

        //  ALWAYS GENERATE NEW REFRESH TOKEN (LOGIN ONLY)
        user.RefreshToken = Guid.NewGuid().ToString();

        var refreshDays = int.Parse(_config["Jwt:RefreshTokenDurationInDays"] ?? "30");
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(refreshDays);

        var accessMinutes = int.Parse(_config["Jwt:DurationInMinutes"] ?? "60");

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = user.RefreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(accessMinutes)
        };
    }


    private async Task<AuthResponse> GenerateStoreTokens(Store store)
    {
        var accessToken = _jwt.GenerateToken(
            store.Id.ToString(),
            "Store",
            store.PhoneNumber
        );

        //  NEW REFRESH TOKEN
        store.RefreshToken = Guid.NewGuid().ToString();

        var refreshDays = int.Parse(_config["Jwt:RefreshTokenDurationInDays"] ?? "30");
        store.RefreshTokenExpiry = DateTime.UtcNow.AddDays(refreshDays);

        var accessMinutes = int.Parse(_config["Jwt:DurationInMinutes"] ?? "60");

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = store.RefreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(accessMinutes)
        };
    }

    private async Task<AuthResponse> GenerateDeliveryTokens(Delivery delivery)
    {
        var accessToken = _jwt.GenerateToken(
            delivery.Id.ToString(),
            "Delivery",
            delivery.PhoneNumber
        );

        // NEW REFRESH TOKEN
        delivery.RefreshToken = Guid.NewGuid().ToString();

        var refreshDays = int.Parse(_config["Jwt:RefreshTokenDurationInDays"] ?? "30");
        delivery.RefreshTokenExpiry = DateTime.UtcNow.AddDays(refreshDays);

        var accessMinutes = int.Parse(_config["Jwt:DurationInMinutes"] ?? "60");

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = delivery.RefreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(accessMinutes)
        };
    }

    // =========================
    // LOGOUT
    // =========================
    //public async Task Logout(Guid userId)
    //{
    //    var user = await _context.Users.FindAsync(userId);

    //    if (user == null)
    //        throw new Exception("User not found");

    //    user.RefreshToken = null;
    //    user.RefreshTokenExpiry = null;

    //    await _context.SaveChangesAsync();
    //}

}