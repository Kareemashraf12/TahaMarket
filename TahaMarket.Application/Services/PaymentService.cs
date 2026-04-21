using Microsoft.EntityFrameworkCore;
using TahaMarket.Application.DTOs;
using TahaMarket.Domain.Entities;
using TahaMarket.Domain.Enums;
using TahaMarket.Infrastructure.Data;

public class PaymentService
{
    private readonly ApplicationDbContext _context;

    public PaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================
    // PAY ORDER (FAKE)
    // =========================
    // =========================
    // PAY ORDER (FAKE)
    // =========================
    public async Task<PaymentResponse> ProcessPayment(PaymentRequest request)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId);

        if (order == null)
            throw new Exception("Order not found");

        if (order.PaymentStatus == PaymentStatus.Paid)
            throw new Exception("Order already paid");

        if (order.PaymentMethod == PaymentMethod.COD)
            throw new Exception("COD orders cannot be paid online");

        if (request.Success)
        {
            order.PaymentStatus = PaymentStatus.Paid;
            order.PaymentTransactionId = Guid.NewGuid().ToString();

            // ✅ FIXED
            order.Status = OrderStatus.Paid;

            await _context.SaveChangesAsync();

            return new PaymentResponse
            {
                OrderId = order.Id,
                PaymentStatus = order.PaymentStatus,
                Message = "Payment successful"
            };
        }
        else
        {
            order.PaymentStatus = PaymentStatus.Failed;
            order.Status = OrderStatus.Cancelled;

            await _context.SaveChangesAsync();

            return new PaymentResponse
            {
                OrderId = order.Id,
                PaymentStatus = order.PaymentStatus,
                Message = "Payment failed"
            };
        }
    }

    // =========================
    // GET PAYMENT STATUS
    // =========================
    public async Task<PaymentResponse> GetStatus(Guid orderId)
    {
        var order = await _context.Orders
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