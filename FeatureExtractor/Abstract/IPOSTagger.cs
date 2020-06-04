using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureExtractor.Abstract
{
    public interface IPOSTagger
    {
        string[] Tag(string[] tokens);
    }
}