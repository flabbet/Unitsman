using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitsmanCore.Converter
{
    public struct Unit
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public UnitTypes UnitType { get; set; }
        public bool UsesSIPrefixes {get;set;}
        public Dictionary<string, double> ConversionTable { get; set; }

        public Unit(string name, string symbol, UnitTypes type, Dictionary<string, double> conversionTable, bool usesSIPrefixes = false)
        {
            Name = name;
            Symbol = symbol;
            UnitType = type;
            UsesSIPrefixes = usesSIPrefixes;
            ConversionTable = conversionTable;
        }
        
    }
}
