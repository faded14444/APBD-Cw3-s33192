namespace LegacyRenewalApp
{
    public class RenewalRequest
    {
        public int CustomerId { get; set; }
        public string PlanCode { get; set; } = string.Empty;
        public int SeatCount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public bool IncludePremiumSupport { get; set; }
        public bool UseLoyaltyPoints { get; set; }
    }
}

