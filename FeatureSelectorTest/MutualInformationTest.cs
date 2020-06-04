using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FeatureSelector;

namespace FeatureSelectorTest
{
    [TestClass]
    public class MutualInformationTest
    {
        [TestMethod]
        public void Calculate()
        {
            //Arrange
            var privateObject = new PrivateObject(new MutualInformation(k: 10));

            int n = 801948;
            double n00 = 774106;
            double n01 = 141;
            double n10 = 27652;
            double n11 = 49;

            //Act
            double value = (double)privateObject.Invoke("Calculate", n, n00, n01, n10, n11);

            //Assert
            Assert.AreEqual(value, 0.01105, 0.1);
        }
    }
}
