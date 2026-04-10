namespace LegacyRenewalApp
{
    public interface IRenewalDispatchService
    {
        void SaveAndNotify(RenewalInvoice invoice, Customer customer, string planCode);
    }

    public class RenewalDispatchService : IRenewalDispatchService
    {
        private readonly IBillingGateway _billingGateway;

        public RenewalDispatchService(IBillingGateway billingGateway)
        {
            _billingGateway = billingGateway;
        }

        public void SaveAndNotify(RenewalInvoice invoice, Customer customer, string planCode)
        {
            _billingGateway.SaveInvoice(invoice);

            if (string.IsNullOrWhiteSpace(customer.Email))
            {
                return;
            }

            string subject = "Subscription renewal invoice";
            string body =
                $"Hello {customer.FullName}, your renewal for plan {planCode} " +
                $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

            _billingGateway.SendEmail(customer.Email, subject, body);
        }
    }
}

