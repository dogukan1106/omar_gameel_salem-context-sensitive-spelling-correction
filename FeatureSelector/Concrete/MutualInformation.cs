using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureSelector
{
    public class MutualInformation : IFeatureSelector
    {
        #region Member Variables

        private readonly int k;

        #endregion

        #region Constructor

        public MutualInformation(int k)
        {
            this.k = k;
        }

        #endregion

        #region IFeatureSelector

        public IDictionary<string, Stats> Select(IDictionary<string, Stats> terms)
        {
            var features = new Dictionary<string, double>(terms.Count, StringComparer.OrdinalIgnoreCase);

            foreach (var term in terms)
            {
                var value = Calculate(n: term.Value.N,
                    n00: term.Value.N00,
                    n01: term.Value.N01,
                    n10: term.Value.N10,
                    n11: term.Value.N11);
                features.Add(term.Key, value);
            }

            var orderedFeatures = features
               .OrderByDescending(s => s.Value)
               .Select(s => s.Key);
            var prunedFeatures = orderedFeatures
                .Take(k)
               .ToDictionary(key => key, key => terms[key]);

            return prunedFeatures;
        }

        #endregion

        #region Private Methods

        private double Calculate(int n, double n00, double n01, double n10, double n11)
        {
            double n1_ = n10 + n11;
            double n_1 = n01 + n11;
            double n0_ = n00 + n01;
            double n_0 = n10 + n00;

            double sum = 0;
            sum += (n11 / n) * Math.Log((n * n11) / (n1_ * n_1), 2);
            sum += (n01 / n) * Math.Log((n * n01) / (n0_ * n_1), 2);
            sum += (n10 / n) * Math.Log((n * n10) / (n1_ * n_0), 2);
            sum += (n00 / n) * Math.Log((n * n00) / (n0_ * n_0), 2);

            return sum;
        }

        #endregion
    }
}