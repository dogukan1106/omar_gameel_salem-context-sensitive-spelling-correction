using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureExtractor
{
    public class CollocationFeaturesExtractor : AbstractExtractor
    {
        public CollocationFeaturesExtractor(int l)
            : base(l)
        {

        }

        public override HashSet<string> Extract(string[] posTags, int targetPosition)
        {
            var features = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int c = 0; c < n; c++)
            {
                var preceding = string.Empty;
                int backward = targetPosition - (c + 1);

                while (backward < targetPosition && backward > -1)
                {
                    preceding += posTags[backward] + " ";
                    backward++;
                }
                if (!string.IsNullOrEmpty(preceding))
                {
                    preceding += "_";
                    AddFeature(features, preceding);
                }

                string succeeding = "_ ";

                int forward = targetPosition + 1;
                for (int j = 0; j <= c && forward < posTags.Length; j++, forward = targetPosition + j + 1)
                {
                    succeeding += posTags[forward] + " ";
                }
                succeeding = succeeding.TrimEnd();
                if (succeeding != "_")
                {
                    AddFeature(features, succeeding.TrimEnd());
                }
            }

            return features;
        }
    }
}