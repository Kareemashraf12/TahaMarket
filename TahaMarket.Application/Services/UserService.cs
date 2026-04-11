using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services.Common;
using TahaMarket.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class UserService
{
    private readonly ApplicationDbContext _context;
    private readonly ImageService _imageService;
    private readonly FileUrlService _fileUrl;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(ApplicationDbContext context, ImageService imageService ,FileUrlService fileUrl, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _imageService = imageService;
        _fileUrl = fileUrl;
        _httpContextAccessor = httpContextAccessor;
    }

    // Get UserId From Token
    private Guid GetUserIdFromToken()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?
            .User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null)
            throw new Exception("Unauthorized");

        return Guid.Parse(userIdClaim);
    }

    //  Get Profile
    public async Task<object> GetProfile(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            throw new Exception("User not found");

        return new
        {
            id = user.Id,
            name = user.Name,
            number = user.PhoneNumber,
            imageUrl = _fileUrl.GetFullUrl(user.ImageUrl)
        };
    }

    //  Update Profile
    public async Task UpdateProfile(Guid userId, UpdateUserProfileRequest request)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            throw new Exception("User not found");

        user.Name = request.Name ?? user.Name;
        user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;

        if (request.Image != null)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(request.Image.FileName);

            var path = Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot/images/users", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await request.Image.CopyToAsync(stream);
            }

            user.ImageUrl = "/images/users/" + fileName;
        }

        await _context.SaveChangesAsync();
    }

    // logOut And refreshToken = Null
    public async Task Logout()
    {
        var userId = GetUserIdFromToken();

        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            throw new Exception("User not found");

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        await _context.SaveChangesAsync();
    }
}