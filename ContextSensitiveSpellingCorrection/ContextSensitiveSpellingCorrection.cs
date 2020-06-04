using FeatureExtractor;
using FeatureExtractor.Abstract;
using FeatureSelector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Winnow;

namespace ContextSensitiveSpellingCorrection
{
    public class ContextSensitiveSpellingCorrection
    {
        #region Member Variables

        int k = 10;//context features
        int l = 2;//collocation features

        private readonly IList<Comparator> _comparators;
        private readonly IPOSTagger _posTagger;
        private readonly ContextFeaturesExtractor _contextFeaturesExtractor;
        private readonly CollocationFeaturesExtractor _collocationtFeaturesExtractor;
        private readonly StatsHelper _statsHelper;
        private Object _lock = new Object();

        #endregion

        #region Constructors

        public ContextSensitiveSpellingCorrection(IPOSTagger posTagger, IEnumerable<string> corpora, IEnumerable<string[]> confusionSets, bool prune)
        {
            _posTagger = posTagger;
            _contextFeaturesExtractor = new ContextFeaturesExtractor(k);
            _collocationtFeaturesExtractor = new CollocationFeaturesExtractor(l);
            _statsHelper = new StatsHelper();
            _comparators = new List<Comparator>(confusionSets.Count());
       
            Sentence[] sentences = PreProcessCorpora(corpora).ToArray();
     

            /*processed corpus was serialized for faster results between trials*/
            XmlSerializer x = new XmlSerializer(typeof(Sentence[]));
            FileStream fs = new FileStream(@"Sentence.xml", FileMode.Open);
            x.Serialize(fs, sentences);
            fs.Close();
            sentences = (Sentence[])x.Deserialize(new FileStream(@"Sentence.xml", FileMode.Open));
            Console.WriteLine("Deserialize complete");

            var featureFrequencies = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

            if (prune)
            {
                /* preprocess terms' frequencies */
                featureFrequencies = _statsHelper.GetFrequencies(sentences);
            }

            Parallel.ForEach(confusionSets, confusionSet =>
            {
                TrainingData output = GenerateTrainingData(sentences, prune, featureFrequencies, confusionSet);

                Train(confusionSet, output.Features.ToArray(), output.Samples);
            });
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks every word in given sentence to check its contextually correct
        /// </summary>
        /// <param name="phrase">sentence to check</param>
        /// <returns> a Dictionary of the wrong word position in sentence as Key and its correction as Value</returns>
        public Dictionary<int, string> Predict(string phrase)
        {
            string[] tokens = SplitIntoWords(phrase);
            var correctWords = new Dictionary<int, string>();

            foreach (var comparator in _comparators)
            {
                foreach (var confusedWord in comparator.ConfusionSet)
                {
                    for (int i = 0; i < tokens.Length; i++)
                    {
                        if (!tokens[i].Equals(confusedWord, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        string[] posTags;

                        lock (_lock)
                        {
                            posTags = _posTagger.Tag(tokens);
                        }

                        var features = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                        features.UnionWith(_contextFeaturesExtractor.Extract(tokens, i));
                        features.UnionWith(_collocationtFeaturesExtractor.Extract(posTags, i));


                        var sample = new Sample
                        {
                            Features = CreateFeaturesVector(features, comparator.Features)
                        };

                        var predictions = new Dictionary<string, double>(comparator.Cloud.Count);

                        foreach (var classifier in comparator.Cloud.Keys)
                        {
                            predictions[classifier] = comparator
                                                 .Cloud[classifier]
                                                 .Sum(c => c.GetScore(sample));
                        }

                        string correctedWord = predictions.Aggregate((a, b) => a.Value > b.Value ? a : b).Key;

                        if (!tokens[i].Equals(correctedWord, StringComparison.OrdinalIgnoreCase))
                        {
                            correctWords.Add(i, correctedWord);
                        }
                    }
                }
            }

            return correctWords;
        }

        #endregion

        #region Private Methods

        private IEnumerable<Sentence> PreProcessCorpora(IEnumerable<string> corpora)
        {
            var sentences = new ConcurrentBag<Sentence>();

            Parallel.ForEach(corpora, phrase =>
            {
                string[] tokens = SplitIntoWords(phrase);
                string[] posTags;

                lock (_lock)
                {
                    posTags = _posTagger.Tag(tokens);
                }

                sentences.Add(new Sentence
                {
                    Words = tokens,
                    POSTags = posTags
                });
            });

            return sentences;
        }

        private TrainingData GenerateTrainingData(Sentence[] sentences, bool prune, Dictionary<string, Dictionary<string, int>> featureFrequencies, IEnumerable<string> confusionSet)
        {
            var allFeatures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var samples = new List<RoughSample>();
            int totalDocumentsCount = sentences.Count();

            /* get exhaustive list of all features */
            foreach (var sentence in sentences)
            {
                foreach (var word in confusionSet)
                {
                    HashSet<string> smallFeatures = ExtractAllFeatures(sentence.Words, sentence.POSTags, word, prune, totalDocumentsCount, featureFrequencies);

                    if (!smallFeatures.Any())//sentence doesnt contain target word
                    {
                        continue;
                    }

                    samples.Add(new RoughSample
                    {
                        Word = word,
                        Features = smallFeatures
                    });

                    allFeatures.UnionWith(smallFeatures);
                }
            }

            Console.WriteLine("Extracting Features for " + confusionSet.Aggregate((a, b) => a + "," + b) + " " + DateTime.Now);

            return new TrainingData
            {
                Samples = samples,
                Features = allFeatures
            };
        }

        private HashSet<string> ExtractAllFeatures(string[] tokens, string[] posTags, string target, bool prune, int totalCount, Dictionary<string, Dictionary<string, int>> featureFrequencies)
        {
            var contextFeatures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var collocationFeatures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < tokens.Length; i++)
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(tokens[i], target))
                {
                    contextFeatures.UnionWith(_contextFeaturesExtractor.Extract(tokens, i));
                    collocationFeatures.UnionWith(_collocationtFeaturesExtractor.Extract(posTags, i));
                }
            }

            if (prune)
            {
                /* get stats */
                Dictionary<string, Stats> stats = _statsHelper.GetFeaturesStats(contextFeatures, featureFrequencies, target, totalCount);

                /* prune */
                contextFeatures = PruneFeatures(stats);
            }

            contextFeatures.UnionWith(collocationFeatures);
            return contextFeatures;
        }

        private string[] SplitIntoWords(string corpus)
        {
            var tokens = corpus
                .Split(new char[] { ',', ' ', '\r', '\n', ':', '-', '"', ';' }, StringSplitOptions.RemoveEmptyEntries);
            return tokens.ToArray();
        }

        private HashSet<string> PruneFeatures(Dictionary<string, Stats> featuresStats)
        {
            var prunedFeatures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            IFeatureSelector _mutualInformationSelector = new MutualInformation(k: 10);
            IFeatureSelector _chiSquaredTestSelector = new ChiSquaredTest(significanceLevel: 0.05);

            prunedFeatures.UnionWith(_mutualInformationSelector.Select(featuresStats).Select(s => s.Key));
            prunedFeatures.UnionWith(_chiSquaredTestSelector.Select(featuresStats).Select(s => s.Key));

            return prunedFeatures;
        }

        private void Train(IEnumerable<string> confusionSet, string[] features, List<RoughSample> trainingData)
        {
            var cloudClassifiers = new Dictionary<string, ISupervisedLearning[]>(confusionSet.Count());

            foreach (var word in confusionSet)
            {
                cloudClassifiers[word] = new ISupervisedLearning[1]
                {
                    new Winnow.Winnow(featuresCount: features.Length, threshold: 1, promotion: 1.5, demotion: 0.5, initialWeight: 1)
                };
            }

            Parallel.ForEach(trainingData, sample =>
            {
                var positive = new Sample
                  {
                      Class = true,
                      Features = CreateFeaturesVector(sample.Features, features)
                  };

                Sample negative = positive.ToggleClass();

                foreach (var cloud in cloudClassifiers)
                {
                    var example = cloud.Key == sample.Word ? positive : negative;

                    foreach (var classifier in cloud.Value)
                    {
                        lock (_lock)
                        {
                            classifier.Train(example);
                        }
                    }
                }
            });

            _comparators.Add(new Comparator(cloudClassifiers, features));           

            Console.WriteLine("Training done for " + confusionSet.Aggregate((a, b) => a + "," + b) + " " + DateTime.Now);
        }

        private bool[] CreateFeaturesVector(HashSet<string> subsetFeatures, string[] allFeatures)
        {
            bool[] featuresVector = new bool[allFeatures.Length];

            for (int i = 0; i < allFeatures.Length; i++)
            {
                if (subsetFeatures.Contains(allFeatures[i], StringComparer.OrdinalIgnoreCase))
                {
                    featuresVector[i] = true;
                }
            }

            return featuresVector;
        }

        #endregion

        private class RoughSample
        {
            public HashSet<string> Features { get; set; }
            public string Word { get; set; }
        }

        private class TrainingData
        {
            public HashSet<string> Features { get; set; }
            public List<RoughSample> Samples { get; set; }
        }
    }
}