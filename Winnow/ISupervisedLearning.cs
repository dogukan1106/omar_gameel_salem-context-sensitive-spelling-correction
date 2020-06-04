using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winnow
{
    public interface ISupervisedLearning
    {
        void Train(IEnumerable<Sample> samples);
        void Train(params Sample[] samples);
        bool Predict(Sample sample);
        double GetScore(Sample sample);
        int Mistakes { get; }
    }
}