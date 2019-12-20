using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnitsmanCore.Converter
{
    public struct Unit
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Include)]
        public string UnitType { get; set; }
        [JsonProperty("usesSIPrefixes")]
        public bool UsesSIPrefixes {get;set;}
        [JsonProperty("conversionTable", DefaultValueHandling = DefaultValueHandling.Populate)]
        public Dictionary<string, double> ConversionTable { get; set; }
        [JsonProperty("symbolDefinitions")]
        public Dictionary<string, string> SymbolDefinitions { get; set; }
        public bool IsComplexType { get => SymbolDefinitions != null; }

        public Unit(string name, string symbol, string type, Dictionary<string, double> conversionTable, bool usesSIPrefixes = false, 
            Dictionary<string,string> symbolDefinitions = null)
        {
            Name = name;
            Symbol = symbol;
            UnitType = type.ToLower();
            UsesSIPrefixes = usesSIPrefixes;
            ConversionTable = conversionTable;
            SymbolDefinitions = symbolDefinitions;
        }
        
    }
}
