using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

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

        public Unit(string name, string symbol, string type, Dictionary<string, double> conversionTable, bool usesSIPrefixes = false)
        {
            Name = name;
            Symbol = symbol;
            UnitType = type.ToLower();
            UsesSIPrefixes = usesSIPrefixes;
            ConversionTable = conversionTable;
        }
        
    }
}
