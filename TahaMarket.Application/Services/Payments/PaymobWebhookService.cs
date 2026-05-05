using Microsoft.EntityFrameworkCore;
using TahaMarket.Domain.Entities;
using TahaMarket.Domain.Enums;
using TahaMarket.Infrastructure.Data;

public class PaymobWebhookService
{
    private readonly ApplicationDbContext _context;

    public PaymobWebhookService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task HandleAsync(dynamic payload)
    {
        try
        {
            var obj = payload?.obj;

            if (obj == null)
                return;

            bool success = obj.success;

            string merchantOrderId = obj.order?.merchant_order_id;
            string transactionId = obj.id;

            if (string.IsNullOrEmpty(merchantOrderId))
                return;

            // =========================
            // FIND ORDER BY MERCHANT ID (IMPORTANT FIX)
            // =========================
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.MerchantOrderId == merchantOrderId);

            if (order == null)
                return;

            // =========================
            // IDEMPOTENCY GUARD
            // =========================
            if (order.PaymentStatus == PaymentStatus.Paid)
                return;

            // =========================
            // UPDATE ORDER BASED ON PAYMENT RESULT
            // =========================
            if (success)
            {
                order.PaymentStatus = PaymentStatus.Paid;
                order.Status = OrderStatus.Paid;
                order.PaymentTransactionId = transactionId;
                order.AcceptedAt = DateTime.UtcNow;
            }
            else
            {
                order.PaymentStatus = PaymentStatus.Failed;
                order.Status = OrderStatus.Pending;
            }

            await _context.SaveChangesAsync();
        }
        catch
        {
            
            return;
        }
    }
}