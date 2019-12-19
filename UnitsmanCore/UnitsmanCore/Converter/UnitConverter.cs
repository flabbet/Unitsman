using System;
using System.Collections.Generic;
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
                ParsedSourceUnit = FindUnit(SourceUnit);
                ParsedTargetUnit = FindUnit(TargetUnit);
                if (!ConversionPossible(ParsedSourceUnit, ParsedTargetUnit))
                {
                    throw new ConversionMismatchException($"Conversion of {ParsedSourceUnit.UnitType.ToString()} to " +
                    $"{ParsedTargetUnit.UnitType.ToString()} is not possible.");
                }

                if (ParsedSourceUnit.ConversionTable.ContainsKey(ParsedTargetUnit.Name))
                {
                    return Value * ParsedSourceUnit.ConversionTable[ParsedTargetUnit.Name];
                }
                throw new ConversionNotFoundException($"Conversion from {ParsedSourceUnit.Name.ToUpperInvariant()} to " +
                    $"{ParsedTargetUnit.Name.ToUpperInvariant()} wasn't found.");

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
                Unit srcUnit = FindUnit(SourceUnit);
                Unit trgtUnit = FindUnit(TargetUnit);
                if (!ConversionPossible(srcUnit, trgtUnit))
                {
                    throw new ConversionMismatchException($"Conversion of {srcUnit.UnitType.ToString()} to " +
                    $"{trgtUnit.UnitType.ToString()} is not possible.");
                }

                if (srcUnit.ConversionTable.ContainsKey(trgtUnit.Name))
                {
                    return Value * srcUnit.ConversionTable[trgtUnit.Name];
                }
                throw new ConversionNotFoundException($"Conversion from {srcUnit.Name.ToUpperInvariant()} to " +
                    $"{trgtUnit.Name.ToUpperInvariant()} wasn't found.");

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
                throw new UnitNotFoundException($"{unit} was not found in units table.");
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

            if (unit1.UnitType == UnitTypes.None && unit2.UnitType == UnitTypes.None)
            {
                return false;
            }
            if(ParsedSourceUnit.UnitType != UnitTypes.None)
            {
                foreach (var conversion in unit1.ConversionTable)
                {
                    if(conversion.Key == unit)
                    {
                        foundUnit.Name = unit;
                        foundUnit.UnitType = unit1.UnitType;
                        return true;
                    }
                }
            }
            if(unit2.UnitType != UnitTypes.None)
            {
                foreach (var conversion in unit2.ConversionTable)
                {
                    if (conversion.Key == unit)
                    {
                        foundUnit.Name = unit;
                        foundUnit.UnitType = unit2.UnitType;
                        return true;
                    }
                }
            }
            return false;
        }
        
    }
}
