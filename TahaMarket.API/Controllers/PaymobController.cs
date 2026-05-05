using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/paymob")]
public class PaymobController : ControllerBase
{
    private readonly PaymobWebhookService _webhookService;

    public PaymobController(PaymobWebhookService webhookService)
    {
        _webhookService = webhookService;
    }

    // =========================
    // WEBHOOK ENDPOINT (PAYMOB)
    // =========================
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] object payload)
    {
        try
        {
            await _webhookService.HandleAsync(payload);
            return Ok();
        }
        catch
        {
            // مهم: Paymob لازم دايمًا يرجع 200
            return Ok();
        }
    }
}