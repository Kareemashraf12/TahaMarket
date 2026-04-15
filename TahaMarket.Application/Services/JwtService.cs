using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class JwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(string id, string role, string phoneNumber)
    {
        var keyString = _config["Jwt:Key"];
        if (string.IsNullOrEmpty(keyString))
            throw new Exception("JWT Key is missing");

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, id),
        new Claim(ClaimTypes.MobilePhone, phoneNumber ?? ""),
        new Claim(ClaimTypes.Role, role.ToString())
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                double.Parse(_config["Jwt:DurationInMinutes"] ?? "60")
            ),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}