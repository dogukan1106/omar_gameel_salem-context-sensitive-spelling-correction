using ContextSensitiveSpellingCorrection;
using FeatureExtractor;
using FeatureExtractor.Abstract;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleClient
{
    class Program
    {
        private class TestCase
        {
            public TestCase(string sentence, string correctWord, string confusionSet)
            {
                this.Sentence = sentence;
                this.CorrectWord = correctWord;
                this.ConfusionSet = confusionSet;
            }

            public string Sentence { get; set; }
            public string CorrectWord { get; set; }
            public string ConfusionSet { get; set; }
        }

        static string _solutionPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        static Stopwatch _sw = new Stopwatch();

        static void Main(string[] args)
        {
            IEnumerable<string[]> confusionSets = GetConfusionSets();
            IEnumerable<string> trainingCorpora = GetCorpora();

          

            Dictionary<string, TestCase[]> testData = GetTestData()
                .GroupBy(s => s.ConfusionSet)
                .ToDictionary(s => s.Key, s => s.ToArray());

            bool prune = false;
            IPOSTagger posTagger = new POSTagger();
            _sw.Start();

            Console.WriteLine("Started...." + DateTime.Now);

            var contextSensitiveSpellingCorrection = new ContextSensitiveSpellingCorrection.ContextSensitiveSpellingCorrection(posTagger, trainingCorpora, confusionSets, prune);

            Console.WriteLine("feature extraction + training took {0} Minutes", _sw.Elapsed.TotalMinutes);
            int totalWrongPredictions = 0;

            Console.WriteLine("Prunning:{0}", prune ? "On" : "Off");

            var csvPath = Path.Combine(_solutionPath, "Results.csv");

            if (File.Exists(csvPath))
            {
                File.Delete(csvPath);
            }

            foreach (var set in testData.Keys)
            {
                int wrongPredictions = 0;

                Parallel.For(0, testData[set].Length, i =>
                {
                    TestCase test = testData[set][i];
                    var wordsList = contextSensitiveSpellingCorrection.Predict(test.Sentence);
                    bool correctAnswer = wordsList.Values.Contains(test.CorrectWord, StringComparer.OrdinalIgnoreCase);

                    if (!correctAnswer)
                    {
                        Interlocked.Increment(ref wrongPredictions);
                        Interlocked.Increment(ref totalWrongPredictions);
                    }
                });

                WriteToCSV(csvPath, set, wrongPredictions, testData[set].Count());
            }

            Console.WriteLine("----------------------------------------------");
            Console.Write("Test ");
            DisplayStats(prune, totalWrongPredictions, testData.Sum(t => t.Value.Count()));
        }

        private static void WriteToCSV(string csvPath, string set, double wrongPredictions, int totalTestsCount)
        {
            var failures = 100 * (wrongPredictions / totalTestsCount);
            var accuracy = 100 - failures;

            using (StreamWriter sw = new StreamWriter(csvPath, true))
            {
                sw.WriteLine(set.Replace(',', '-') + "," + accuracy);
            }
        }

        private static void DisplayStats(bool prune, double wrongPredictions, double count)
        {
            var failures = 100 * (wrongPredictions / count);
            Console.WriteLine("Accuracy: {0:00} % of {1} test samples, took: {2:00 Minutes}", 100 - failures, count, _sw.Elapsed.TotalMinutes);
        }

        private static IEnumerable<string[]> GetConfusionSets()
        {
            IEnumerable<string[]> confusionSets = new List<string[]> 
            {
                new string[2] { "peace", "piece" }
                ,new string[2] { "where", "were" } 
                ,new string[2] { "hour", "our" } 
                ,new string[3] { "by", "buy","bye" } 
                ,new string[3] { "cite", "site","sight" } 
                ,new string[2] { "coarse", "course" } 
                ,new string[2] { "desert", "dessert" } 
                ,new string[2] { "knew", "new" } 
                ,new string[2] { "hear", "here" } 
                ,new string[3] { "vain", "vane","vein" } 
                ,new string[2] { "loose", "lose" } 
                ,new string[2] { "plaine", "plane" } 
                ,new string[2] { "principal", "principle" } 
                ,new string[2] { "sea", "see" } 
                ,new string[3] { "quiet", "quit", "quite"} 
                ,new string[3] { "rain", "reign", "rein"} 
                ,new string[2] { "waist", "waste" } 
                ,new string[2] { "weak", "week" }   
                ,new string[2] { "weather", "whether" }
                ,new string[2] { "fourth", "forth" } 
                ,new string[2] { "passed", "past" } 
                ,new string[2] { "council", "counsel" } 
                ,new string[2] { "complement", "compliment" } 
                ,new string[2] { "their", "there" } 
                ,new string[2] { "later", "latter" } 
                ,new string[2] { "threw", "through" } 
                ,new string[3] { "to", "too","two" } 
                ,new string[2] { "brake", "break" } 
            };

            return confusionSets;
        }

        private static IEnumerable<string> GetCorpora()
        {
            string corpus = File
            .ReadAllText(Path.Combine(_solutionPath, @"Corpus\Release Corpus.txt"));

            return corpus.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries); ;
        }

        private static IEnumerable<TestCase> GetTestData()
        {
            IEnumerable<string> lines = File.ReadAllText(Path.Combine(_solutionPath, @"Corpus\Test.txt"))
           .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                int pipeIdx = line.LastIndexOf('|');
                int commaIdx = pipeIdx;
                for (; commaIdx > -1; commaIdx--)
                {
                    if (line[commaIdx] == ',')
                    {
                        break;
                    }
                }

                string sentence = line.Substring(0, commaIdx);
                string correctWord = line.Substring(commaIdx + 1, pipeIdx - (commaIdx + 1));
                string confusionSet = line.Substring(pipeIdx + 1);

                yield return new TestCase(sentence, correctWord, confusionSet);
            }
        }
    }
}