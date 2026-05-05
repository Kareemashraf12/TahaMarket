using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

public class PaymobSecurityService
{
    private readonly string _hmacSecret;

    public PaymobSecurityService(IConfiguration config)
    {
        _hmacSecret = config["Paymob:HmacSecret"];
    }

    public bool IsValidSignature(string payload, string receivedHmac)
    {
        if (string.IsNullOrEmpty(receivedHmac))
            return false;

        var computed = ComputeHmac(payload);

        return FixedTimeEquals(computed, receivedHmac);
    }

    private string ComputeHmac(string data)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_hmacSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));

        return Convert.ToHexString(hash).ToLower();
    }

    private bool FixedTimeEquals(string a, string b)
    {
        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);

        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }
}