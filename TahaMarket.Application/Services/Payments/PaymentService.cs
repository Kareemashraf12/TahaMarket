using Microsoft.EntityFrameworkCore;
using TahaMarket.Application.DTOs;
using TahaMarket.Domain.Enums;
using TahaMarket.Infrastructure.Data;

public class PaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly PaymobService _paymobService;

    public PaymentService(
        ApplicationDbContext context,
        PaymobService paymobService)
    {
        _context = context;
        _paymobService = paymobService;
    }

    // =========================
    // CREATE ONLINE PAYMENT SESSION
    // =========================
    public async Task<PaymentUrlResponse> CreateOnlinePayment(Guid orderId)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new Exception("Order not found");

        if (order.PaymentStatus == PaymentStatus.Paid)
            throw new Exception("Order already paid");

        if (order.PaymentMethod == PaymentMethod.COD)
            throw new Exception("COD order cannot use online payment");

        // =========================
        // Generate UNIQUE merchant id
        // =========================
        var merchantOrderId = $"{order.Id}-{Guid.NewGuid().ToString().Substring(0, 6)}";

        // save it (IMPORTANT)
        order.MerchantOrderId = merchantOrderId;

        await _context.SaveChangesAsync();

        // =========================
        // Create payment URL
        // =========================
        var paymentUrl = await _paymobService.CreatePaymentUrl(
            amount: order.FinalPrice,
            merchantOrderId: merchantOrderId
        );

        return new PaymentUrlResponse
        {
            OrderId = order.Id,
            PaymentUrl = paymentUrl
        };
    }

    // =========================
    // GET STATUS
    // =========================
    public async Task<PaymentResponse> GetStatus(Guid orderId)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new Exception("Order not found");

        return new PaymentResponse
        {
            OrderId = order.Id,
            PaymentStatus = order.PaymentStatus,
            Message = order.PaymentStatus.ToString()
        };
    }
}