using System;
using System.Collections.Generic;
namespace FeatureSelector
{
    public interface IFeatureSelector
    {
        IDictionary<string, Stats> Select(IDictionary<string, Stats> terms);
    }
}