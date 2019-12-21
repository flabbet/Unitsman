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

        public List<Unit> Units { get; set; }
        public double Value { get; set; }
        public string SourceUnit { get; set; }
        public string TargetUnit { get; set; }

        public Unit ParsedSourceUnit { get; private set; }
        public Unit ParsedTargetUnit { get; private set; }
        UnitFinder Finder { get; set; }


        public UnitConverter(List<Unit> units, double value, string sourceUnit, string targetUnit)
        {
            Units = units;
            Value = value;
            SourceUnit = sourceUnit;
            TargetUnit = targetUnit;
            Finder = new UnitFinder(Units);
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

                ParsedSourceUnit = Finder.FindUnit(sourceUnit, ParsedTargetUnit);
                ParsedTargetUnit = Finder.FindUnit(targetUnit, ParsedSourceUnit);


                if (!ConversionPossible(ParsedSourceUnit, ParsedTargetUnit))
                {
                    throw new ConversionMismatchException($"Conversion of {ParsedSourceUnit.UnitType} to " +
                    $"{ParsedTargetUnit.UnitType.ToString()} is not possible.");
                }

                if (ParsedSourceUnit.ConversionTable.ContainsKey(ParsedTargetUnit.Name) || 
                    ParsedSourceUnit.ConversionTable.ContainsKey(ParsedTargetUnit.Symbol))
                {
                    string key = ParsedSourceUnit.ConversionTable.ContainsKey(ParsedTargetUnit.Name) ? ParsedTargetUnit.Name : ParsedTargetUnit.Symbol;
                    double conversionVal = ParsedSourceUnit.ConversionTable[key];
                    if (reverseVal)
                        return 1/(value * conversionVal);
                    return value * conversionVal;
                }
                if (ParsedTargetUnit.ConversionTable.ContainsKey(ParsedSourceUnit.Name) ||
                    ParsedTargetUnit.ConversionTable.ContainsKey(ParsedTargetUnit.Symbol))
                {
                    string key = ParsedTargetUnit.ConversionTable.ContainsKey(ParsedSourceUnit.Name) ? ParsedSourceUnit.Name : ParsedSourceUnit.Symbol;
                    double conversionVal = ParsedTargetUnit.ConversionTable[key];
                    if (!Units.Contains(ParsedTargetUnit) && reverseVal == false) 
                        return value * conversionVal;
                     return  1 / (value * conversionVal);
                }
                else
                {
                    if (Finder.TryGetConversion(ParsedSourceUnit, ParsedTargetUnit.Name, out Tuple<string, double> generatedConversion))
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
                catch //If classic conversion failed, tries smart complex conversion
                {
                    if (OneComplexUnitIsKnown(ParsedSourceUnit, ParsedTargetUnit))
                    {
                        Unit definedComplexUnit = ParsedSourceUnit.IsComplexType ? ParsedSourceUnit : ParsedTargetUnit;
                        if (HasSameFormula(definedComplexUnit, targetUnit))
                        {
                            var units = BreakDownComplexExpression(targetUnit, definedComplexUnit, reverseVal, value);
                            return EvaluateFormula(definedComplexUnit, units, reverseVal, value);
                        }
                    }

                    throw ex;
                }
            }
        }

        private Dictionary<string,string> BreakDownComplexExpression(string targetUnit, Unit definedComplexUnit, bool reverseVal, double value)
        {
            Dictionary<string, string> convertedPrimitiveUnits = new Dictionary<string, string>();
            string[] extractedSymbols = targetUnit.Split("/");
            for (int i = 0; i < definedComplexUnit.SymbolDefinitions.Count; i++)
            {
                string key = definedComplexUnit.SymbolDefinitions.ElementAt(i).Key;
                if(key == extractedSymbols[i])
                {
                    convertedPrimitiveUnits.Add(key, "1");
                    continue;
                }
                convertedPrimitiveUnits.Add(key, Convert(1, extractedSymbols[i], key, true).ToString());
            }
            return convertedPrimitiveUnits;
            
        }

        private string[] GetUnitsFromRawFormula(string formula)
        {
            return formula.Split('/');
        }

        private double EvaluateFormula(Unit definedComplexUnit, Dictionary<string,string> convertedPrimitiveUnits, bool reverseVal, double units)
        {
            string formulaWithValues = FillFormula(definedComplexUnit.Symbol, convertedPrimitiveUnits);
            Expression e = new Expression(formulaWithValues);
            double expressionVal = (double)e.Evaluate();
            if (!reverseVal)
                return units * (1 / expressionVal);
            return units * expressionVal;
        }

        private string FillFormula(string formula, Dictionary<string, string> values)
        {
            string finalFormula = formula;
            foreach (var value in values)
            {
                var regex = new Regex(Regex.Escape(value.Key));
                finalFormula = regex.Replace(finalFormula, value.Value);
            }
            return finalFormula.Replace(',','.');
        }

        private bool HasSameFormula(Unit definedUnit, string targetUnit)
        {
            return definedUnit.SymbolDefinitions.Count == targetUnit.Split('/').Length;
        }

        private bool OneComplexUnitIsKnown(Unit unit1, Unit unit2)
        {
            return unit1.IsComplexType && unit2.Symbol == null || unit2.IsComplexType && unit1.Symbol == null;
        }

        private bool ConversionPossible(Unit srcUnit, Unit targetUnit)
        {
            return srcUnit.UnitType == targetUnit.UnitType;
        }      
    }
}
