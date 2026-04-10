using System;

namespace LegacyRenewalApp
{
    public interface IRenewalRequestValidator
    {
        RenewalRequest ValidateAndNormalize(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints);
    }

    public class RenewalRequestValidator : IRenewalRequestValidator
    {
        public RenewalRequest ValidateAndNormalize(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            if (customerId <= 0)
            {
                throw new ArgumentException("Customer id must be positive");
            }

            if (string.IsNullOrWhiteSpace(planCode))
            {
                throw new ArgumentException("Plan code is required");
            }

            if (seatCount <= 0)
            {
                throw new ArgumentException("Seat count must be positive");
            }

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                throw new ArgumentException("Payment method is required");
            }

            return new RenewalRequest
            {
                CustomerId = customerId,
                PlanCode = planCode.Trim().ToUpperInvariant(),
                SeatCount = seatCount,
                PaymentMethod = paymentMethod.Trim().ToUpperInvariant(),
                IncludePremiumSupport = includePremiumSupport,
                UseLoyaltyPoints = useLoyaltyPoints
            };
        }
    }
}

