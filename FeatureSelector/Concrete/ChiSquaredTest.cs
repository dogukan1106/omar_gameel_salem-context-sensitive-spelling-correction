﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureSelector
{
    public class ChiSquaredTest : IFeatureSelector
    {
        #region Member Variables

        private readonly double _significanceLevel;
        private readonly Dictionary<double, double> _distributionTable;

        #endregion

        #region Constructor

        public ChiSquaredTest(double significanceLevel)
        {
            this._significanceLevel = significanceLevel;
            _distributionTable = new Dictionary<double, double> { 
                {0.1,2.71},
                {0.05,3.84},
                {0.01,6.63},
                {0.005,7.88},
                {0.001,10.83}
            };
        }

        #endregion

        #region IFeatureSelector

        public IDictionary<string, Stats> Select(IDictionary<string, Stats> features)
        {
            var prunedFeatures = new Dictionary<string, Stats>(StringComparer.OrdinalIgnoreCase);

            foreach (var feature in features)
            {
                var featureValue = Calculate(feature.Value.N, feature.Value.N00, feature.Value.N01, feature.Value.N10, feature.Value.N11);

                if (featureValue > _distributionTable[_significanceLevel])
                {
                    prunedFeatures.Add(feature.Key, feature.Value);
                }
            }

            return prunedFeatures;
        }

        #endregion

        #region Private Methods

        private double Calculate(int n, double n00, double n01, double n10, double n11)
        {
            var numerator = n * Math.Pow(((n11 * n00) - (n10 * n01)), 2);
            var denominator = (n11 + n01) *
                (n11 + n10) *
                (n10 + n00) *
                (n01 + n00);
            var value = numerator / denominator;
            return value;
        }

        #endregion
    }
}