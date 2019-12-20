using NCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnitsmanCore.Exceptions;

namespace UnitsmanCore.Converter
{
    public class UnitConverter
    {

        const string UnknownSymbol = "Unknown";

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

        public double Convert(double value, string sourceUnit, string targetUnit, bool reverseVal = false)
        {
            try
            {
                if (reverseVal)
                {
                    ParsedSourceUnit = new Unit();
                    ParsedTargetUnit = new Unit();
                    var tmp = targetUnit;
                    targetUnit = sourceUnit;
                    sourceUnit = tmp;
                }

                ParsedSourceUnit = FindUnit(sourceUnit);
                ParsedTargetUnit = FindUnit(targetUnit);


                if (!ConversionPossible(ParsedSourceUnit, ParsedTargetUnit))
                {
                    throw new ConversionMismatchException($"Conversion of {ParsedSourceUnit.UnitType} to " +
                    $"{ParsedTargetUnit.UnitType.ToString()} is not possible.");
                }

                if (ParsedSourceUnit.ConversionTable.ContainsKey(ParsedTargetUnit.Name))
                {
                    double conversionVal = ParsedSourceUnit.ConversionTable[ParsedTargetUnit.Name];
                    if (reverseVal)
                        return 1/(value * conversionVal);
                    return value * conversionVal;
                }
                if (ParsedTargetUnit.ConversionTable.ContainsKey(ParsedSourceUnit.Name))
                {
                    double conversionVal = ParsedTargetUnit.ConversionTable[ParsedSourceUnit.Name];
                    if (!Units.Contains(ParsedTargetUnit) && reverseVal == false) 
                        return value * conversionVal;
                     return  1 / (value * conversionVal);
                }
                else
                {
                    if (TryGetConversion(ParsedSourceUnit, ParsedTargetUnit.Name, out Tuple<string, double> generatedConversion))
                    {
                        return value * generatedConversion.Item2;
                    }
                }

                throw new ConversionNotFoundException($"Conversion from {ParsedSourceUnit.Name.ToUpperInvariant()} to " +
                    $"{ParsedTargetUnit.Name.ToUpperInvariant()} wasn't found.");

            }
            catch (ConversionException ex)
            {
                try
                {
                    if (reverseVal == false && string.IsNullOrEmpty(ParsedSourceUnit.Name))
                    {
                        return value * Convert(1, sourceUnit, targetUnit, true); //When source unit is not defined, retries by replacing them.
                    }
                    throw ex;
                }
                catch
                {
                    if (ComplexConversionRequired(ParsedSourceUnit, ParsedTargetUnit))
                    {
                        Unit definedComplexUnit = ParsedSourceUnit.IsComplexType ? ParsedSourceUnit : ParsedTargetUnit;
                        if (HasSameFormula(definedComplexUnit, targetUnit))
                        {
                            return ConvertComplexExpression(targetUnit, definedComplexUnit, reverseVal, value);
                        }
                    }
                    throw ex;
                }
            }
        }

        private double ConvertComplexExpression(string targetUnit, Unit definedComplexUnit, bool reverseVal, double value)
        {
            Dictionary<string, double> convertedPrimitiveUnits = new Dictionary<string, double>();
            string[] extractedSymbols = targetUnit.Split("/");
            for (int i = 0; i < definedComplexUnit.SymbolDefinitions.Count; i++)
            {
                string key = definedComplexUnit.SymbolDefinitions.ElementAt(i).Key;
                if(key == extractedSymbols[i])
                {
                    convertedPrimitiveUnits.Add(key, 1);
                    continue;
                }
                convertedPrimitiveUnits.Add(key, Convert(1, extractedSymbols[i], key, true));
            }
            string formulaWithValues = FillFormula(definedComplexUnit.Symbol, convertedPrimitiveUnits);
            Expression e = new Expression(formulaWithValues);
            double expressionVal = (double)e.Evaluate();
            if (!reverseVal)
                return value * (1 / expressionVal);
            return value * expressionVal;
        }

        private string FillFormula(string formula, Dictionary<string, double> values)
        {
            string finalFormula = formula;
            foreach (var value in values)
            {
                var regex = new Regex(Regex.Escape(value.Key));
                finalFormula = regex.Replace(finalFormula, value.Value.ToString());
            }
            return finalFormula.Replace(',','.');
        }

        private bool HasSameFormula(Unit definedUnit, string targetUnit)
        {
            return definedUnit.SymbolDefinitions.Count == targetUnit.Split('/').Length;
        }

        private bool ComplexConversionRequired(Unit unit1, Unit unit2)
        {
            return unit1.IsComplexType && unit2.Symbol == null || unit2.IsComplexType && unit1.Symbol == null;
        }

        private bool ConversionPossible(Unit srcUnit, Unit targetUnit)
        {
            return srcUnit.UnitType == targetUnit.UnitType;
        }

        public Unit FindUnit(string unit)
        {
            Unit foundUnit = Units.Find(x => x.Name == unit || x.Symbol == unit);
            if (string.IsNullOrEmpty(foundUnit.UnitType))
            {
                if (TryGenerateFromConversionTable(unit, ParsedSourceUnit, ParsedTargetUnit, out Unit generatedUnit))
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

        private Dictionary<string, double> MultiplyDictionaryValues(Dictionary<string, double> dictionary, double multiplier)
        {
            Dictionary<string, double> finalDictionary = dictionary;
            for(int i = 0; i < finalDictionary.Count; i++)
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

        private bool TryGetConversion(Unit srcUnit, string targetUnit, out Tuple<string, double> conversion)
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

        private bool TryGenerateFromConversionTable(string unit, Unit unit1, Unit unit2, out Unit foundUnit)
        {
            foundUnit = new Unit
            {
                Symbol = UnknownSymbol,
                UnitType = "",
                UsesSIPrefixes = false,
                ConversionTable = new Dictionary<string, double>()
            };

            Unit deepSearchTargetUnit = new Unit();
            if (!string.IsNullOrEmpty(unit1.UnitType))
            {
                deepSearchTargetUnit = unit1;
            }
            else if (!string.IsNullOrEmpty(unit2.UnitType))
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
