using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winnow
{
    /// <summary>
    /// Algorithm for learning monotone disjunction function from labeled examples
    /// </summary>
    public class Winnow : ISupervisedLearning
    {
        #region Member Variables

        private readonly double[] _weights;
        private readonly double _threshold;
        private readonly double _promotion;
        private readonly double _demotion;

        #endregion

        #region Constructor

        public Winnow(int featuresCount, double threshold, double promotion, double demotion, double initialWeight)
        {
            this._threshold = threshold;
            this._promotion = promotion;
            this._demotion = demotion;
            this._weights = new double[featuresCount];

            for (int i = 0; i < _weights.Length; i++)
            {
                _weights[i] = initialWeight;
            }
        }

        #endregion

        #region ISupervisedLearning

        public void Train(IEnumerable<Sample> samples)
        {
            Train(samples.ToArray());
        }

        public void Train(params Sample[] samples)
        {
            foreach (var s in samples)
            {
                bool prediction = Predict(s);

                if (prediction != s.Class)//prediction was wrong
                {
                    Mistakes++;

                    if (!prediction && s.Class)
                    {
                        AdjustWeights(s, _promotion);
                    }
                    else
                    {
                        AdjustWeights(s, _demotion);
                    }
                }
            }
        }

        public bool Predict(Sample sample)
        {
            double sum = GetScore(sample);

            return sum >= _threshold;
        }

        public double GetScore(Sample sample)
        {
            double sum = 0;

            for (int i = 0; i < _weights.Length; i++)
            {
                if (sample.Features[i])
                {
                    sum += _weights[i];
                }
            }

            return sum;
        }

        public int Mistakes { get; private set; }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var item in _weights)
            {
                sb.Append(item);
                sb.Append(",");
            }

            return sb.ToString();
        }

        #endregion

        #region Private Methods

        private void AdjustWeights(Sample s, double adjustment)
        {
            for (int i = 0; i < _weights.Length; i++)
            {
                if (s.Features[i])
                {
                    _weights[i] *= adjustment;
                }
            }
        }

        #endregion

    }
}