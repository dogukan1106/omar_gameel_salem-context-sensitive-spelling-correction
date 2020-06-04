using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FeatureExtractor
{
    public class ContextFeaturesExtractor : AbstractExtractor
    {
        public ContextFeaturesExtractor(int k)
            : base(k)
        {

        }

        public override HashSet<string> Extract(string[] tokens, int targetPosition)
        {
            var features = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int backward = targetPosition - 1;
            int forward = targetPosition + 1;

            for (int counter = 0; counter < n; counter++)
            {
                if (backward <= -1 && forward >= tokens.Length)
                {
                    break;
                }

                if (backward > -1)
                {
                    AddFeature(features, tokens[backward]);
                    backward--;
                }

                if (forward < tokens.Length)
                {
                    AddFeature(features, tokens[forward]);
                    forward++;
                }
            }

            return features;
        }
    }
}
