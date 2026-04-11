using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TahaMarket.Application.Services.Common
{
    public class DeliveryPricingService
    {
        private const decimal BaseFee = 10;      
        private const decimal PricePerKm = 3;
        private const decimal MinFee = 15;

        public decimal CalculateFee(double distanceKm)
        {
            var fee = BaseFee + ((decimal)distanceKm * PricePerKm);

            if (fee < MinFee)
                fee = MinFee;

            return Math.Round(fee, 2);
        }
    }
}
