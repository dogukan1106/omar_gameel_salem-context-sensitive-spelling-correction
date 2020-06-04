using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FeatureSelector;
using System.Collections.Generic;
using System.Linq;

namespace FeatureSelectorTest
{
    [TestClass]
    public class ChiSquaredTestTest
    {
        private readonly IFeatureSelector target = new ChiSquaredTest(significanceLevel: 0.05);

        [TestMethod]
        public void Calculate()
        {
            //Arrange
            var privateObject = new PrivateObject(target);

            int n = 801948;
            double n00 = 774106;
            double n01 = 141;
            double n10 = 27652;
            double n11 = 49;

            //Act
            double value = (double)privateObject.Invoke("Calculate", n, n00, n01, n10, n11);

            //Assert
            Assert.AreEqual(value, 284, 0.5);
        }
        [TestMethod]
        public void InDependent()
        {
            IDictionary<string, Stats> actual = target.Select(new Dictionary<string, Stats> { { "test", new Stats { N00 = 25, N10 = 14, N11 = 36, N01 = 30, N = 105 } } });

            Assert.IsFalse(actual.Any());
        }

        [TestMethod]
        public void Dependent()
        {
            IDictionary<string, Stats> actual = target.Select(new Dictionary<string, Stats> { { "test", new Stats { N00 = 15, N10 = 42, N11 = 10, N01 = 33, N = 100 } } });

            Assert.IsTrue(actual.Any());
        }
    }
}
