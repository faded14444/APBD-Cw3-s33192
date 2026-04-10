using System;
using System.Collections.Generic;
using System.Linq;

namespace LegacyRenewalApp
{
    public interface IRenewalInvoiceFactory
    {
        RenewalInvoice Create(
            RenewalRequest request,
            Customer customer,
            decimal baseAmount,
            decimal discountAmount,
            decimal supportFee,
            decimal paymentFee,
            decimal taxAmount,
            decimal finalAmount,
            IEnumerable<string> notes);
    }

    public class RenewalInvoiceFactory : IRenewalInvoiceFactory
    {
        public RenewalInvoice Create(
            RenewalRequest request,
            Customer customer,
            decimal baseAmount,
            decimal discountAmount,
            decimal supportFee,
            decimal paymentFee,
            decimal taxAmount,
            decimal finalAmount,
            IEnumerable<string> notes)
        {
            string notesText = string.Join(" ", notes.Select(x => $"{x};"));

            return new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{request.CustomerId}-{request.PlanCode}",
                CustomerName = customer.FullName,
                PlanCode = request.PlanCode,
                PaymentMethod = request.PaymentMethod,
                SeatCount = request.SeatCount,
                BaseAmount = Math.Round(baseAmount, 2, MidpointRounding.AwayFromZero),
                DiscountAmount = Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero),
                SupportFee = Math.Round(supportFee, 2, MidpointRounding.AwayFromZero),
                PaymentFee = Math.Round(paymentFee, 2, MidpointRounding.AwayFromZero),
                TaxAmount = Math.Round(taxAmount, 2, MidpointRounding.AwayFromZero),
                FinalAmount = Math.Round(finalAmount, 2, MidpointRounding.AwayFromZero),
                Notes = notesText.Trim(),
                GeneratedAt = DateTime.UtcNow
            };
        }
    }
}

