using System;
using System.Collections.Generic;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        private readonly IRenewalRequestValidator _requestValidator;
        private readonly ICustomerRepository _customerRepository;
        private readonly ISubscriptionPlanRepository _planRepository;
        private readonly IDiscountCalculator _discountCalculator;
        private readonly ISupportFeeResolver _supportFeeResolver;
        private readonly IPaymentFeeCalculator _paymentFeeCalculator;
        private readonly ITaxRateResolver _taxRateResolver;
        private readonly IRenewalInvoiceFactory _invoiceFactory;
        private readonly IRenewalDispatchService _dispatchService;

        public SubscriptionRenewalService()
            : this(
                new RenewalRequestValidator(),
                new CustomerRepository(),
                new SubscriptionPlanRepository(),
                new DiscountCalculator(CreateDiscountPolicies()),
                new SupportFeeResolver(CreateSupportFeePolicies()),
                new PaymentFeeCalculator(CreatePaymentFeePolicies()),
                new TaxRateResolver(CreateTaxPolicies()),
                new RenewalInvoiceFactory(),
                new RenewalDispatchService(new LegacyBillingGatewayAdapter()))
        {
        }

        public SubscriptionRenewalService(
            IRenewalRequestValidator requestValidator,
            ICustomerRepository customerRepository,
            ISubscriptionPlanRepository planRepository,
            IDiscountCalculator discountCalculator,
            ISupportFeeResolver supportFeeResolver,
            IPaymentFeeCalculator paymentFeeCalculator,
            ITaxRateResolver taxRateResolver,
            IRenewalInvoiceFactory invoiceFactory,
            IRenewalDispatchService dispatchService)
        {
            _requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _planRepository = planRepository ?? throw new ArgumentNullException(nameof(planRepository));
            _discountCalculator = discountCalculator ?? throw new ArgumentNullException(nameof(discountCalculator));
            _supportFeeResolver = supportFeeResolver ?? throw new ArgumentNullException(nameof(supportFeeResolver));
            _paymentFeeCalculator = paymentFeeCalculator ?? throw new ArgumentNullException(nameof(paymentFeeCalculator));
            _taxRateResolver = taxRateResolver ?? throw new ArgumentNullException(nameof(taxRateResolver));
            _invoiceFactory = invoiceFactory ?? throw new ArgumentNullException(nameof(invoiceFactory));
            _dispatchService = dispatchService ?? throw new ArgumentNullException(nameof(dispatchService));
        }

        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            var request = _requestValidator.ValidateAndNormalize(
                customerId,
                planCode,
                seatCount,
                paymentMethod,
                includePremiumSupport,
                useLoyaltyPoints);

            var customer = _customerRepository.GetById(request.CustomerId);
            var plan = _planRepository.GetByCode(request.PlanCode);

            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
            }

            decimal baseAmount = (plan.MonthlyPricePerSeat * request.SeatCount * 12m) + plan.SetupFee;
            var discountResult = _discountCalculator.Calculate(
                new DiscountCalculationContext
                {
                    Customer = customer,
                    Plan = plan,
                    BaseAmount = baseAmount,
                    SeatCount = request.SeatCount,
                    UseLoyaltyPoints = request.UseLoyaltyPoints
                });

            decimal discountAmount = discountResult.TotalAmount;
            var notes = new List<string>(discountResult.Notes);

            decimal subtotalAfterDiscount = baseAmount - discountAmount;
            if (subtotalAfterDiscount < 300m)
            {
                subtotalAfterDiscount = 300m;
                notes.Add("minimum discounted subtotal applied");
            }

            decimal supportFee = 0m;
            if (request.IncludePremiumSupport)
            {
                supportFee = _supportFeeResolver.Resolve(request.PlanCode);
                notes.Add("premium support included");
            }

            FeeResult paymentFeeResult = _paymentFeeCalculator.Calculate(
                request.PaymentMethod,
                subtotalAfterDiscount + supportFee);

            decimal paymentFee = paymentFeeResult.Amount;
            notes.Add(paymentFeeResult.Note);

            decimal taxRate = _taxRateResolver.Resolve(customer.Country);
            decimal taxBase = subtotalAfterDiscount + supportFee + paymentFee;
            decimal taxAmount = taxBase * taxRate;
            decimal finalAmount = taxBase + taxAmount;

            if (finalAmount < 500m)
            {
                finalAmount = 500m;
                notes.Add("minimum invoice amount applied");
            }

            var invoice = _invoiceFactory.Create(
                request,
                customer,
                baseAmount,
                discountAmount,
                supportFee,
                paymentFee,
                taxAmount,
                finalAmount,
                notes);

            _dispatchService.SaveAndNotify(invoice, customer, request.PlanCode);
            return invoice;
        }

        private static IEnumerable<IDiscountPolicy> CreateDiscountPolicies()
        {
            return new IDiscountPolicy[]
            {
                new SilverSegmentDiscountPolicy(),
                new GoldSegmentDiscountPolicy(),
                new PlatinumSegmentDiscountPolicy(),
                new EducationSegmentDiscountPolicy(),
                new LongTermLoyaltyDiscountPolicy(),
                new BasicLoyaltyDiscountPolicy(),
                new LargeTeamDiscountPolicy(),
                new MediumTeamDiscountPolicy(),
                new SmallTeamDiscountPolicy(),
                new LoyaltyPointsDiscountPolicy()
            };
        }

        private static IEnumerable<ISupportFeePolicy> CreateSupportFeePolicies()
        {
            return new ISupportFeePolicy[]
            {
                new StartSupportFeePolicy(),
                new ProSupportFeePolicy(),
                new EnterpriseSupportFeePolicy()
            };
        }

        private static IEnumerable<IPaymentFeePolicy> CreatePaymentFeePolicies()
        {
            return new IPaymentFeePolicy[]
            {
                new CardPaymentFeePolicy(),
                new BankTransferPaymentFeePolicy(),
                new PaypalPaymentFeePolicy(),
                new InvoicePaymentFeePolicy()
            };
        }

        private static IEnumerable<ITaxPolicy> CreateTaxPolicies()
        {
            return new ITaxPolicy[]
            {
                new PolandTaxPolicy(),
                new GermanyTaxPolicy(),
                new CzechRepublicTaxPolicy(),
                new NorwayTaxPolicy()
            };
        }
    }
}
