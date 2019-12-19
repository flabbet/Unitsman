using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnitsmanCore.Exceptions;

namespace UnitsmanCore.Converter
{
    public class UnitConverter
    {

        public List<Unit> Units { get; set; }
        public double Value { get; set; }
        public string SourceUnit { get; set; }
        public string TargetUnit { get; set; }

        public Unit ParsedSourceUnit { get; private set; }
        public Unit ParsedTargetUnit { get; private set; }

        public UnitConverter(List<Unit> units, double value, string sourceUnit, string targetUnit)
        {
            Units = units;
            Value = value;
            SourceUnit = sourceUnit;
            TargetUnit = targetUnit;
        }


        public double Convert()
        {
            try
            {
                return Convert(Value, SourceUnit, TargetUnit);
            }
            catch (ConversionException ex)
            {
                throw ex;
            }
        }

        public double Convert(double value, string sourceUnit, string targetUnit)
        {
            try
            {
                ParsedSourceUnit = FindUnit(sourceUnit);
                ParsedTargetUnit = FindUnit(targetUnit);
                if (!ConversionPossible(ParsedSourceUnit, ParsedTargetUnit))
                {
                    throw new ConversionMismatchException($"Conversion of {ParsedSourceUnit.UnitType.ToString()} to " +
                    $"{ParsedTargetUnit.UnitType.ToString()} is not possible.");
                }

                if (ParsedSourceUnit.ConversionTable.ContainsKey(ParsedTargetUnit.Name))
                {
                    return value * ParsedSourceUnit.ConversionTable[ParsedTargetUnit.Name];
                }
                else if (ParsedTargetUnit.ConversionTable.ContainsKey(ParsedSourceUnit.Name))
                {
                    return value * ParsedTargetUnit.ConversionTable[ParsedSourceUnit.Name];
                }

                throw new ConversionNotFoundException($"Conversion from {ParsedSourceUnit.Name.ToUpperInvariant()} to " +
                    $"{ParsedTargetUnit.Name.ToUpperInvariant()} wasn't found.");

            }
            catch (ConversionException ex)
            {
                throw ex;
            }
        }

        private bool ConversionPossible(Unit srcUnit, Unit targetUnit)
        {
            return srcUnit.UnitType == targetUnit.UnitType;
        }

        public Unit FindUnit(string unit)
        {           
           Unit foundUnit = Units.Find(x => x.Name == unit || x.Symbol == unit);
            if(foundUnit.UnitType == UnitTypes.None)
            {
                if (TryGetFromConversionTable(unit, ParsedSourceUnit, ParsedTargetUnit, out Unit generatedUnit))
                {
                    return generatedUnit;
                }
                throw new UnitNotFoundException($"{unit.ToUpperInvariant()} was not found in units table.");
            }
            return foundUnit;
        }

        private bool TryGetFromConversionTable(string unit, Unit unit1, Unit unit2 ,out Unit foundUnit)
        {
            foundUnit = new Unit
            {
                Symbol = "Unknown",
                UnitType = UnitTypes.None,
                UsesSIPrefixes = false,
                ConversionTable = new Dictionary<string, double>()
            };

            Unit deepSearchTargetUnit = new Unit();
            if(unit1.UnitType != UnitTypes.None)
            {
                deepSearchTargetUnit = unit1;
            }
            else if(unit2.UnitType != UnitTypes.None)
            {
                deepSearchTargetUnit = unit2;
            }
            else
            {
                return false;
            }

            DeepConversionResult resultDeepConversion = DeepConversionSearch(deepSearchTargetUnit, unit);
            foundUnit.Name = unit;
            foundUnit.UnitType = resultDeepConversion.Value.UnitType;
            if (resultDeepConversion.WasSuccessful)
            {
                Tuple<string, double> conversionRecord = GenerateConversionTableFromPath(deepSearchTargetUnit, unit, resultDeepConversion.ConversionPath);
                foundUnit.ConversionTable.Add(conversionRecord.Item1, conversionRecord.Item2);
            }
            return resultDeepConversion.WasSuccessful;
        }

        private Tuple<string, double> GenerateConversionTableFromPath(Unit startingUnit, string targetUnit, List<Unit> conversionPath)
        {
            string key = startingUnit.Name;
            double val = 0;

            for(int i = 0; i < conversionPath.Count; i++) 
            {
                val = 1 / conversionPath[i].ConversionTable[key];
                key = conversionPath[i].Name;
                if(i == conversionPath.Count - 1)
                {
                    val *= conversionPath[i].ConversionTable[targetUnit];
                }
            }

            return new Tuple<string, double>(startingUnit.Name, val);
        }

        private DeepConversionResult DeepConversionSearch(Unit currentUnit, string unitToFind, List<Unit> conversionPath = null)
        {
            if(conversionPath == null)
            {
                conversionPath = new List<Unit>();
            }

            if(UnitContainsConversion(currentUnit, unitToFind))
            {
                return new DeepConversionResult(currentUnit, conversionPath, true);
            }

            foreach (var conversion in currentUnit.ConversionTable)
            {
                if (conversionPath.Contains(currentUnit)) continue;
                IEnumerable<Unit> matchingUnits = Units.Where(x => x.Name == conversion.Key);
                if (matchingUnits.Count() > 0)
                {
                    conversionPath.Add(matchingUnits.First());
                    return DeepConversionSearch(matchingUnits.First(), unitToFind, conversionPath);
                }
            }
            return new DeepConversionResult(new Unit(), conversionPath, false);
        }

        private bool UnitContainsConversion(Unit unit, string targetUnit)
        {
            foreach (var conversion in unit.ConversionTable)
            {
                if (conversion.Key == targetUnit)
                {
                    return true;
                }
            }
            return false;
        }
        
    }
}
