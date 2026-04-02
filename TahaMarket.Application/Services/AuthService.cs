using TahaMarket.Infrastructure.Data;
using TahaMarket.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using TahaMarket.Application.DTOs;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtService _jwt;

    public AuthService(ApplicationDbContext context, JwtService jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    // ------------------- REGISTER -------------------
    public async Task<AuthResponse?> Register(RegisterRequest request)
    {
        var exists = await _context.Users
            .AnyAsync(u => u.PhoneNumber == request.PhoneNumber);

        if (exists)
            return null;

        var user = new User
        {
            PhoneNumber = request.PhoneNumber,
            Name = request.Name,
            Email = request.Email,
            UserType = "Customer",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return await GenerateUserTokens(user);
    }

    // ------------------- LOGIN (USER + STORE) -------------------
    public async Task<AuthResponse?> Login(LoginRequest request)
    {
        var phone = request.PhoneNumber.Trim();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == phone);

        if (user != null)
        {
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            return await GenerateUserTokens(user);
        }

        var store = await _context.Stores
            .FirstOrDefaultAsync(s => s.PhoneNumber == phone);

        if (store != null)
        {
            if (!BCrypt.Net.BCrypt.Verify(request.Password, store.PasswordHash))
                return null;

            return await GenerateStoreTokens(store);
        }

        return null;
    }

    // ------------------- USER TOKENS -------------------
    private async Task<AuthResponse> GenerateUserTokens(User user)
    {
        var accessToken = _jwt.GenerateToken(
            user.Id.ToString(),
            user.UserType,
            user.PhoneNumber
        );

        var refreshToken = Guid.NewGuid().ToString();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(60)
        };
    }

    // ------------------- STORE TOKENS -------------------
    private async Task<AuthResponse> GenerateStoreTokens(Store store)
    {
        var accessToken = _jwt.GenerateToken(
            store.Id.ToString(),
            "Store",
            store.PhoneNumber
        );

        var refreshToken = Guid.NewGuid().ToString();

        store.RefreshToken = refreshToken;
        store.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(60)
        };
    }
}