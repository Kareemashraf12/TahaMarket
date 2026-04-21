using Microsoft.AspNetCore.Mvc;
using TahaMarket.Application.DTOs;

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly PaymentService _service;

    public PaymentController(PaymentService service)
    {
        _service = service;
    }

    // =========================
    // PAY ORDER
    // =========================
    [HttpPost("pay")]
    public async Task<IActionResult> Pay([FromBody] PaymentRequest request)
    {
        if (request == null)
            return BadRequest("Invalid request");

        try
        {
            var result = await _service.ProcessPayment(request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                Message = ex.Message
            });
        }
    }
    // =========================
    // GET STATUS
    // =========================
    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetStatus(Guid orderId)
    {
        var result = await _service.GetStatus(orderId);
        return Ok(result);
    }
}