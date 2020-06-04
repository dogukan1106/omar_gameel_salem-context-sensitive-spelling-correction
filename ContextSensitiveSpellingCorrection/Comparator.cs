using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winnow;

namespace ContextSensitiveSpellingCorrection
{
    class Comparator
    {
        public Dictionary<string, ISupervisedLearning[]> Cloud { get; set; }

        public string[] Features { get; set; }

        public IEnumerable<string> ConfusionSet
        {
            get
            {
                return Cloud.Keys;
            }
        }

        public Comparator(Dictionary<string, ISupervisedLearning[]> cloud, string[] features)
        {
            this.Cloud = cloud;
            this.Features = features;
        }
    }
}