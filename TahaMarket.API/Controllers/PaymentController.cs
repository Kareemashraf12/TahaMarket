using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public PaymentController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    // =========================
    // CREATE PAYMENT SESSION
    // =========================
    [HttpPost("create/{orderId}")]
    public async Task<IActionResult> Create(Guid orderId)
    {
        try
        {
            var result = await _paymentService.CreateOnlinePayment(orderId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    // =========================
    // GET STATUS
    // =========================
    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetStatus(Guid orderId)
    {
        try
        {
            var result = await _paymentService.GetStatus(orderId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
    }
}