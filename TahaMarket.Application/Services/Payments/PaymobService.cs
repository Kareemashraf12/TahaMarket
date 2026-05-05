using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

public class PaymobService
{
    private readonly HttpClient _http;

    private readonly string _apiKey;
    private readonly int _integrationId;
    private readonly int _iframeId;

    public PaymobService(HttpClient http, IConfiguration config)
    {
        _http = http;

        var paymob = config.GetSection("Paymob");

        _apiKey = paymob["ApiKey"] ?? throw new Exception("Paymob ApiKey missing");
        _integrationId = int.Parse(paymob["IntegrationId"] ?? "0");
        _iframeId = int.Parse(paymob["IframeId"] ?? "0");
    }

    // =========================
    // PUBLIC ENTRY
    // =========================
    public async Task<string> CreatePaymentUrl(decimal amount, string merchantOrderId)
    {
        var token = await GetAuthToken();

        var orderId = await CreateOrder(token, amount, merchantOrderId);

        var paymentToken = await CreatePaymentKey(token, amount, orderId);

        return $"https://accept.paymob.com/api/acceptance/iframes/{_iframeId}?payment_token={paymentToken}";
    }

    // =========================
    // AUTH
    // =========================
    private async Task<string> GetAuthToken()
    {
        var res = await _http.PostAsync(
            "https://accept.paymob.com/api/auth/tokens",
            JsonContent(new { api_key = _apiKey })
        );

        var json = await Read(res);

        if (!json.RootElement.TryGetProperty("token", out var tokenElement))
            throw new Exception("AUTH FAILED: invalid response");

        return tokenElement.GetString()
            ?? throw new Exception("AUTH FAILED: token is null");
    }

    // =========================
    // ORDER
    // =========================
    private async Task<int> CreateOrder(string token, decimal amount, string merchantOrderId)
    {
        var res = await _http.PostAsync(
            "https://accept.paymob.com/api/ecommerce/orders",
            JsonContent(new
            {
                auth_token = token,
                delivery_needed = false,
                amount_cents = (int)(amount * 100),
                currency = "EGP",
                merchant_order_id = merchantOrderId
            })
        );

        var json = await Read(res);

        if (!json.RootElement.TryGetProperty("id", out var idElement))
            throw new Exception("ORDER FAILED: invalid response");

        return idElement.GetInt32();
    }

    // =========================
    // PAYMENT KEY
    // =========================
    private async Task<string> CreatePaymentKey(string token, decimal amount, int orderId)
    {
        var res = await _http.PostAsync(
            "https://accept.paymob.com/api/acceptance/payment_keys",
            JsonContent(new
            {
                auth_token = token,
                amount_cents = (int)(amount * 100),
                expiration = 3600,
                order_id = orderId,
                integration_id = _integrationId,
                currency = "EGP",
                billing_data = new
                {
                    first_name = "User",
                    last_name = "Test",
                    email = "test@test.com",
                    phone_number = "01000000000",
                    apartment = "NA",
                    floor = "NA",
                    street = "NA",
                    building = "NA",
                    shipping_method = "NA",
                    postal_code = "NA",
                    city = "Cairo",
                    country = "EG",
                    state = "NA"
                }
            })
        );

        var json = await Read(res);

        if (!json.RootElement.TryGetProperty("token", out var paymentToken))
            throw new Exception("PAYMENT KEY FAILED: invalid response");

        return paymentToken.GetString()
            ?? throw new Exception("PAYMENT KEY FAILED: token null");
    }

    // =========================
    // HELPERS
    // =========================
    private static StringContent JsonContent(object obj)
        => new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    private static async Task<JsonDocument> Read(HttpResponseMessage res)
    {
        var raw = await res.Content.ReadAsStringAsync();

        if (!res.IsSuccessStatusCode)
            throw new Exception($"HTTP ERROR: {(int)res.StatusCode} - {raw}");

        return JsonDocument.Parse(raw);
    }
}