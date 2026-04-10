using System;
using System.Collections.Generic;
using System.Linq;

namespace LegacyRenewalApp
{
    public class DiscountCalculationContext
    {
        public Customer Customer { get; set; } = new Customer();
        public SubscriptionPlan Plan { get; set; } = new SubscriptionPlan();
        public decimal BaseAmount { get; set; }
        public int SeatCount { get; set; }
        public bool UseLoyaltyPoints { get; set; }
    }

    public class DiscountComponent
    {
        public decimal Amount { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    public class DiscountResult
    {
        public decimal TotalAmount { get; set; }
        public IList<string> Notes { get; set; } = new List<string>();
    }

    public interface IDiscountPolicy
    {
        string Group { get; }
        int Priority { get; }
        bool IsApplicable(DiscountCalculationContext context);
        DiscountComponent Calculate(DiscountCalculationContext context);
    }

    public interface IDiscountCalculator
    {
        DiscountResult Calculate(DiscountCalculationContext context);
    }

    public class DiscountCalculator : IDiscountCalculator
    {
        private readonly IReadOnlyList<string> _groupOrder = new[] { "Segment", "Tenure", "TeamSize", "LoyaltyPoints" };
        private readonly IReadOnlyCollection<IDiscountPolicy> _policies;

        public DiscountCalculator(IEnumerable<IDiscountPolicy> policies)
        {
            _policies = policies.ToList();
        }

        public DiscountResult Calculate(DiscountCalculationContext context)
        {
            var result = new DiscountResult();

            foreach (string group in _groupOrder)
            {
                var policy = _policies
                    .Where(p => p.Group == group)
                    .OrderBy(p => p.Priority)
                    .FirstOrDefault(p => p.IsApplicable(context));

                if (policy == null)
                {
                    continue;
                }

                var component = policy.Calculate(context);
                result.TotalAmount += component.Amount;
                if (!string.IsNullOrWhiteSpace(component.Note))
                {
                    result.Notes.Add(component.Note);
                }
            }

            return result;
        }
    }

    public class SilverSegmentDiscountPolicy : IDiscountPolicy
    {
        public string Group => "Segment";
        public int Priority => 10;

        public bool IsApplicable(DiscountCalculationContext context) => context.Customer.Segment == "Silver";

        public DiscountComponent Calculate(DiscountCalculationContext context)
        {
            return new DiscountComponent { Amount = context.BaseAmount * 0.05m, Note = "silver discount" };
        }
    }

    public class GoldSegmentDiscountPolicy : IDiscountPolicy
    {
        public string Group => "Segment";
        public int Priority => 20;

        public bool IsApplicable(DiscountCalculationContext context) => context.Customer.Segment == "Gold";

        public DiscountComponent Calculate(DiscountCalculationContext context)
        {
            return new DiscountComponent { Amount = context.BaseAmount * 0.10m, Note = "gold discount" };
        }
    }

    public class PlatinumSegmentDiscountPolicy : IDiscountPolicy
    {
        public string Group => "Segment";
        public int Priority => 30;

        public bool IsApplicable(DiscountCalculationContext context) => context.Customer.Segment == "Platinum";

        public DiscountComponent Calculate(DiscountCalculationContext context)
        {
            return new DiscountComponent { Amount = context.BaseAmount * 0.15m, Note = "platinum discount" };
        }
    }

    public class EducationSegmentDiscountPolicy : IDiscountPolicy
    {
        public string Group => "Segment";
        public int Priority => 40;

        public bool IsApplicable(DiscountCalculationContext context)
        {
            return context.Customer.Segment == "Education" && context.Plan.IsEducationEligible;
        }

        public DiscountComponent Calculate(DiscountCalculationContext context)
        {
            return new DiscountComponent { Amount = context.BaseAmount * 0.20m, Note = "education discount" };
        }
    }

    public class LongTermLoyaltyDiscountPolicy : IDiscountPolicy
    {
        public string Group => "Tenure";
        public int Priority => 10;

        public bool IsApplicable(DiscountCalculationContext context) => context.Customer.YearsWithCompany >= 5;

        public DiscountComponent Calculate(DiscountCalculationContext context)
        {
            return new DiscountComponent { Amount = context.BaseAmount * 0.07m, Note = "long-term loyalty discount" };
        }
    }

    public class BasicLoyaltyDiscountPolicy : IDiscountPolicy
    {
        public string Group => "Tenure";
        public int Priority => 20;

        public bool IsApplicable(DiscountCalculationContext context) => context.Customer.YearsWithCompany >= 2;

        public DiscountComponent Calculate(DiscountCalculationContext context)
        {
            return new DiscountComponent { Amount = context.BaseAmount * 0.03m, Note = "basic loyalty discount" };
        }
    }

    public class LargeTeamDiscountPolicy : IDiscountPolicy
    {
        public string Group => "TeamSize";
        public int Priority => 10;

        public bool IsApplicable(DiscountCalculationContext context) => context.SeatCount >= 50;

        public DiscountComponent Calculate(DiscountCalculationContext context)
        {
            return new DiscountComponent { Amount = context.BaseAmount * 0.12m, Note = "large team discount" };
        }
    }

    public class MediumTeamDiscountPolicy : IDiscountPolicy
    {
        public string Group => "TeamSize";
        public int Priority => 20;

        public bool IsApplicable(DiscountCalculationContext context) => context.SeatCount >= 20;

        public DiscountComponent Calculate(DiscountCalculationContext context)
        {
            return new DiscountComponent { Amount = context.BaseAmount * 0.08m, Note = "medium team discount" };
        }
    }

    public class SmallTeamDiscountPolicy : IDiscountPolicy
    {
        public string Group => "TeamSize";
        public int Priority => 30;

        public bool IsApplicable(DiscountCalculationContext context) => context.SeatCount >= 10;

        public DiscountComponent Calculate(DiscountCalculationContext context)
        {
            return new DiscountComponent { Amount = context.BaseAmount * 0.04m, Note = "small team discount" };
        }
    }

    public class LoyaltyPointsDiscountPolicy : IDiscountPolicy
    {
        public string Group => "LoyaltyPoints";
        public int Priority => 10;

        public bool IsApplicable(DiscountCalculationContext context)
        {
            return context.UseLoyaltyPoints && context.Customer.LoyaltyPoints > 0;
        }

        public DiscountComponent Calculate(DiscountCalculationContext context)
        {
            int pointsToUse = Math.Min(context.Customer.LoyaltyPoints, 200);
            return new DiscountComponent { Amount = pointsToUse, Note = $"loyalty points used: {pointsToUse}" };
        }
    }
}

