using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winnow;

namespace WinnowTest
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Sample> samples = new List<Sample>(100);
            int featureCount = 10;
            Random rand = new Random(DateTime.Now.Millisecond);

            for (int i = 0; i < 100; i++)
            {
                bool[] features = new bool[featureCount];

                for (int j = 0; j < featureCount; j++)
                {
                    features[j] = rand.Next(11) >= 5;
                }

                Sample s = new Sample
                {
                    Features = features,
                    Class = function(features)
                };

                samples.Add(s);
            }

            ISupervisedLearning winnow = new Winnow.Winnow(featureCount, featureCount, 2, .5, 1);
            winnow.Train(samples.Take(70));
            int error = 0;

            foreach (var item in samples.Skip(70))
            {
                if (winnow.Predict(item) != item.Class)
                {
                    error++;
                }
            }

            Console.WriteLine(string.Format("{0}% error rate in {1} samples", 100 * (error / 30), 30));
        }

        private static bool function(bool[] features)
        {
            return features[3] || features[6] || features[7] || features[9];
        }
    }
}
