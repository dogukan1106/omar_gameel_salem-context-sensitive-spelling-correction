using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureExtractor
{
    public abstract class AbstractExtractor
    {
        protected readonly int n;

        public AbstractExtractor(int n)
        {
            this.n = n;
        }

        public abstract HashSet<string> Extract(string[] tokens, int targetPosition);

        protected void AddFeature(HashSet<string> features, string feature)
        {
            if (!features.Contains(feature))
            {
                features.Add(feature);
            }
        }
    }
}