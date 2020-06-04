using FeatureExtractor.Abstract;
using OpenNLP.Tools.PosTagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureExtractor
{
    public class POSTagger : IPOSTagger
    {
        private readonly EnglishMaximumEntropyPosTagger _posTagger;

        public POSTagger()
        {
            string modelsPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            modelsPath = Path.Combine(modelsPath, "models");
            _posTagger = new EnglishMaximumEntropyPosTagger(Path.Combine(modelsPath, "EnglishPOS.nbin"), Path.Combine(modelsPath, "tagdict"));
        }

        public string[] Tag(string[] tokens)
        {
            return _posTagger.Tag(tokens);
        }
    }
}