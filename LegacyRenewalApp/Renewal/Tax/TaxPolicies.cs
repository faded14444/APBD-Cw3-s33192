using System.Collections.Generic;
using System.Linq;

namespace LegacyRenewalApp
{
    public interface ITaxPolicy
    {
        bool CanHandle(string country);
        decimal TaxRate { get; }
    }

    public interface ITaxRateResolver
    {
        decimal Resolve(string country);
    }

    public class TaxRateResolver : ITaxRateResolver
    {
        private readonly IReadOnlyCollection<ITaxPolicy> _policies;

        public TaxRateResolver(IEnumerable<ITaxPolicy> policies)
        {
            _policies = policies.ToList();
        }

        public decimal Resolve(string country)
        {
            ITaxPolicy policy = _policies.FirstOrDefault(x => x.CanHandle(country));
            return policy == null ? 0.20m : policy.TaxRate;
        }
    }

    public class PolandTaxPolicy : ITaxPolicy
    {
        public bool CanHandle(string country) => country == "Poland";
        public decimal TaxRate => 0.23m;
    }

    public class GermanyTaxPolicy : ITaxPolicy
    {
        public bool CanHandle(string country) => country == "Germany";
        public decimal TaxRate => 0.19m;
    }

    public class CzechRepublicTaxPolicy : ITaxPolicy
    {
        public bool CanHandle(string country) => country == "Czech Republic";
        public decimal TaxRate => 0.21m;
    }

    public class NorwayTaxPolicy : ITaxPolicy
    {
        public bool CanHandle(string country) => country == "Norway";
        public decimal TaxRate => 0.25m;
    }
}

