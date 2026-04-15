namespace TahaMarket.Application.Services.Common
{
    public class DeliveryPricingService
    {
        // Default values (fallback only)
        private decimal _baseFee = 10m;
        private decimal _pricePerKm = 3m;
        private decimal _minFee = 15m;

        // =========================
        // SET FROM ADMIN (later DB/config)
        // =========================
        public void SetPricing(decimal baseFee, decimal pricePerKm, decimal minFee)
        {
            _baseFee = baseFee;
            _pricePerKm = pricePerKm;
            _minFee = minFee;
        }

        // =========================
        // CALCULATE DELIVERY FEE
        // =========================
        public decimal CalculateFee(double distanceKm)
        {
            var fee = _baseFee + ((decimal)distanceKm * _pricePerKm);

            if (fee < _minFee)
                fee = _minFee;

            return Math.Round(fee, 2);
        }
    }
}