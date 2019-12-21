using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnitsmanCore.Exceptions;

namespace UnitsmanCore.Converter
{
    public class UnitFinder
    {

        const string UnknownSymbol = "Unknown";

        public List<Unit> Units { get; set; }

        public UnitFinder(List<Unit> units)
        {
            Units = units;
        }

        public Unit FindUnit(string unit, Unit startingPoint)
        {
            Unit foundUnit = Units.Find(x => x.Name == unit || x.Symbol == unit);
            if (string.IsNullOrEmpty(foundUnit.UnitType))
            {
                if (TryGenerateFromConversionTable(unit, startingPoint, out Unit generatedUnit))
                {
                    return generatedUnit;
                }
                else
                {
                    if (SIPrefixEquivalentUnitExists(unit))
                    {
                        Unit baseUnit = GetUnitWithoutSIPrefix(unit);
                        double multiplier = Math.Pow(10, (int)Enum.Parse(typeof(SIPrefixes), GetSIPrefix(unit)));

                        Unit finalUnit = new Unit(unit, unit, baseUnit.UnitType, MultiplyDictionaryValues(baseUnit.ConversionTable, multiplier));
                        return baseUnit;
                    }
                }
                throw new UnitNotFoundException($"{unit.ToUpperInvariant()} was not found in units table.");
            }
            return foundUnit;
        }

        public Unit FindUnit(string unit)
        {
            return FindUnit(unit, SearchUnitWhereReferenced(unit));
        }

        public bool TryFindUnit(string unit, out Unit foundUnit)
        {
            foundUnit = new Unit();
            try
            {
                foundUnit = FindUnit(unit);
                return true;
            }
            catch (UnitNotFoundException)
            {
                return false;
            }
        }

        public bool TryFindUnit(string unit, Unit startingPoint, out Unit foundUnit)
        {
            foundUnit = new Unit();
            try
            {
                foundUnit = FindUnit(unit, startingPoint);
                return true;
            }
            catch (UnitNotFoundException)
            {
                return false;
            }
        }

        public Unit SearchUnitWhereReferenced(string unitName)
        {
            return Units.Find(x => x.ConversionTable.ContainsKey(unitName));
        }

        private Dictionary<string, double> MultiplyDictionaryValues(Dictionary<string, double> dictionary, double multiplier)
        {
            Dictionary<string, double> finalDictionary = dictionary;
            for (int i = 0; i < finalDictionary.Count; i++)
            {
                string key = finalDictionary.ElementAt(i).Key;
                finalDictionary[key] *= multiplier;
            }
            return finalDictionary;
        }

        private string GetSIPrefix(string unitSymbol)
        {
            return unitSymbol.Length > 2 ? "da" : unitSymbol[0].ToString();
        }

        private bool SIPrefixEquivalentUnitExists(string unitSymbol)
        {
            if (unitSymbol.Length < 2) return false; //Si prefix + unit is at least 2 chars long

            Unit foundUnit = GetUnitWithoutSIPrefix(unitSymbol);
            return !string.IsNullOrEmpty(foundUnit.UnitType);
        }

        private Unit GetUnitWithoutSIPrefix(string unit)
        {
            string unitWithoutPrefix = unit.Length > 2 ? unit.Substring(2, unit.Length - 2) : unit.Substring(1, unit.Length - 1);

            Unit foundUnit = Units.Find(x => x.UsesSIPrefixes == true && x.Symbol == unitWithoutPrefix);
            return foundUnit;
        }

        public bool TryGetConversion(Unit srcUnit, string targetUnit, out Tuple<string, double> conversion)
        {
            conversion = new Tuple<string, double>("", 0);
            DeepConversionResult resultDeepConversion = DeepConversionSearch(srcUnit, targetUnit);
            if (resultDeepConversion.WasSuccessful)
            {
                Tuple<string, double> conversionRecord = GenerateConversionTableFromPath(srcUnit, targetUnit, resultDeepConversion.ConversionPath);
                conversion = conversionRecord;
            }
            return resultDeepConversion.WasSuccessful;
        }

        private bool TryGenerateFromConversionTable(string targetUnit, Unit currentUnit, out Unit foundUnit)
        {
            foundUnit = new Unit
            {
                Symbol = UnknownSymbol,
                UnitType = "",
                UsesSIPrefixes = false,
                ConversionTable = new Dictionary<string, double>()
            };

            if (string.IsNullOrEmpty(currentUnit.UnitType)) return false;

            DeepConversionResult resultDeepConversion = DeepConversionSearch(currentUnit, targetUnit);
            foundUnit.Name = targetUnit;
            foundUnit.UnitType = resultDeepConversion.Value.UnitType;
            if (resultDeepConversion.WasSuccessful)
            {
                Tuple<string, double> conversionRecord = GenerateConversionTableFromPath(currentUnit, targetUnit, resultDeepConversion.ConversionPath);
                foundUnit.ConversionTable.Add(conversionRecord.Item1, conversionRecord.Item2);
            }
            return resultDeepConversion.WasSuccessful;
        }

        private Tuple<string, double> GenerateConversionTableFromPath(Unit startingUnit, string targetUnit, List<Unit> conversionPath)
        {
            string key = startingUnit.Name;
            double val = 1;

            for (int i = 0; i < conversionPath.Count; i++)
            {
                if (i + 1 < conversionPath.Count)
                {
                    key = conversionPath[i + 1].Name;
                }
                else
                {
                    key = targetUnit;
                }
                val *= conversionPath[i].ConversionTable[key];
            }

            return new Tuple<string, double>(startingUnit.Name, val);
        }

        private DeepConversionResult DeepConversionSearch(Unit currentUnit, string unitToFind, List<Unit> conversionPath = null)
        {
            DeepConversionResult res = new DeepConversionResult();
            if (conversionPath == null)
            {
                conversionPath = new List<Unit>();
                conversionPath.Add(currentUnit);
            }

            if (UnitContainsConversion(currentUnit, unitToFind))
            {
                return new DeepConversionResult(currentUnit, conversionPath, true);
            }

            foreach (var conversion in currentUnit.ConversionTable)
            {
                List<Unit> matchingUnits = Units.Where(x => x.Name == conversion.Key && !conversionPath.Contains(x)).ToList();
                if (matchingUnits.Count() > 0)
                {
                    conversionPath.Add(matchingUnits.Last());
                    res = DeepConversionSearch(matchingUnits[0], unitToFind, conversionPath);
                    if (res.WasSuccessful) return res;
                    else
                    {
                        conversionPath.Remove(matchingUnits.Last());
                    }
                }
            }
            return new DeepConversionResult(new Unit(), conversionPath, false);
        }

        private bool UnitContainsConversion(Unit unit, string targetUnit)
        {
            if (unit.ConversionTable == null) return false;
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
