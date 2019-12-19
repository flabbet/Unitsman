using System;
using System.Collections.Generic;
using System.Text;

namespace UnitsmanCore.Converter
{
    public struct DeepConversionResult
    {
        public List<Unit> ConversionPath { get;set; }
        public Unit Value { get; set; }
        public bool WasSuccessful { get; set; }

        public DeepConversionResult(Unit value, List<Unit> conversionPath, bool wasSuccessfull)
        {
            Value = value;
            ConversionPath = conversionPath;
            WasSuccessful = wasSuccessfull;
        }
    }
}
