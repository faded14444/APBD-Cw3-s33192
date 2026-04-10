using System;
using System.Collections.Generic;
using System.Linq;

namespace LegacyRenewalApp
{
    public class FeeResult
    {
        public decimal Amount { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    public interface ISupportFeePolicy
    {
        bool CanHandle(string planCode);
        decimal ResolveFee();
    }

    public interface ISupportFeeResolver
    {
        decimal Resolve(string planCode);
    }

    public class SupportFeeResolver : ISupportFeeResolver
    {
        private readonly IReadOnlyCollection<ISupportFeePolicy> _policies;

        public SupportFeeResolver(IEnumerable<ISupportFeePolicy> policies)
        {
            _policies = policies.ToList();
        }

        public decimal Resolve(string planCode)
        {
            ISupportFeePolicy policy = _policies.FirstOrDefault(x => x.CanHandle(planCode));
            return policy == null ? 0m : policy.ResolveFee();
        }
    }

    public class StartSupportFeePolicy : ISupportFeePolicy
    {
        public bool CanHandle(string planCode) => planCode == "START";
        public decimal ResolveFee() => 250m;
    }

    public class ProSupportFeePolicy : ISupportFeePolicy
    {
        public bool CanHandle(string planCode) => planCode == "PRO";
        public decimal ResolveFee() => 400m;
    }

    public class EnterpriseSupportFeePolicy : ISupportFeePolicy
    {
        public bool CanHandle(string planCode) => planCode == "ENTERPRISE";
        public decimal ResolveFee() => 700m;
    }

    public interface IPaymentFeePolicy
    {
        bool CanHandle(string paymentMethod);
        FeeResult Calculate(decimal amountBeforePaymentFee);
    }

    public interface IPaymentFeeCalculator
    {
        FeeResult Calculate(string paymentMethod, decimal amountBeforePaymentFee);
    }

    public class PaymentFeeCalculator : IPaymentFeeCalculator
    {
        private readonly IReadOnlyCollection<IPaymentFeePolicy> _policies;

        public PaymentFeeCalculator(IEnumerable<IPaymentFeePolicy> policies)
        {
            _policies = policies.ToList();
        }

        public FeeResult Calculate(string paymentMethod, decimal amountBeforePaymentFee)
        {
            var policy = _policies.FirstOrDefault(x => x.CanHandle(paymentMethod));
            if (policy == null)
            {
                throw new ArgumentException("Unsupported payment method");
            }

            return policy.Calculate(amountBeforePaymentFee);
        }
    }

    public class CardPaymentFeePolicy : IPaymentFeePolicy
    {
        public bool CanHandle(string paymentMethod) => paymentMethod == "CARD";

        public FeeResult Calculate(decimal amountBeforePaymentFee)
        {
            return new FeeResult
            {
                Amount = amountBeforePaymentFee * 0.02m,
                Note = "card payment fee"
            };
        }
    }

    public class BankTransferPaymentFeePolicy : IPaymentFeePolicy
    {
        public bool CanHandle(string paymentMethod) => paymentMethod == "BANK_TRANSFER";

        public FeeResult Calculate(decimal amountBeforePaymentFee)
        {
            return new FeeResult
            {
                Amount = amountBeforePaymentFee * 0.01m,
                Note = "bank transfer fee"
            };
        }
    }

    public class PaypalPaymentFeePolicy : IPaymentFeePolicy
    {
        public bool CanHandle(string paymentMethod) => paymentMethod == "PAYPAL";

        public FeeResult Calculate(decimal amountBeforePaymentFee)
        {
            return new FeeResult
            {
                Amount = amountBeforePaymentFee * 0.035m,
                Note = "paypal fee"
            };
        }
    }

    public class InvoicePaymentFeePolicy : IPaymentFeePolicy
    {
        public bool CanHandle(string paymentMethod) => paymentMethod == "INVOICE";

        public FeeResult Calculate(decimal amountBeforePaymentFee)
        {
            return new FeeResult
            {
                Amount = 0m,
                Note = "invoice payment"
            };
        }
    }
}

