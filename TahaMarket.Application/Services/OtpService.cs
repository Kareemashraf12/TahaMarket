using Microsoft.EntityFrameworkCore;
using TahaMarket.Domain.Entities;
using TahaMarket.Infrastructure.Data;

public class OtpService
{
    private readonly ApplicationDbContext _context;

    public OtpService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Generate OTP
    public async Task<string> GenerateOtp(string phone)
    {
        var code = new Random().Next(100000, 999999).ToString();

        var otp = new OtpCode
        {
            PhoneNumber = phone,
            Code = code,
            Expiry = DateTime.UtcNow.AddMinutes(2),
            IsUsed = false
        };

        _context.OtpCodes.Add(otp);
        await _context.SaveChangesAsync();

        return code;
    }

    // Verify OTP
    public async Task<bool> VerifyOtp(string phone, string code)
    {
        var otp = await _context.OtpCodes
            .Where(o => o.PhoneNumber == phone && o.Code == code)
            .OrderByDescending(o => o.Expiry)
            .FirstOrDefaultAsync();

        if (otp == null)
            return false;

        if (otp.IsUsed || otp.Expiry < DateTime.UtcNow)
            return false;

        otp.IsUsed = true;
        await _context.SaveChangesAsync();

        return true;
    }

    // Resend OTP (delete old + create new)
    public async Task<string> ResendOtp(string phone)
    {
        var oldOtps = _context.OtpCodes
            .Where(o => o.PhoneNumber == phone);

        _context.OtpCodes.RemoveRange(oldOtps);

        return await GenerateOtp(phone);
    }
}