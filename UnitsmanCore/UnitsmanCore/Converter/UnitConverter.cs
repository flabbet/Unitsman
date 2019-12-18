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
                Unit sourceUnit = FindUnit(SourceUnit);
                Unit targetUnit = FindUnit(TargetUnit);
                if (!ConversionPossible(sourceUnit, targetUnit))
                {
                    throw new ConversionMismatchException($"Conversion of {sourceUnit.UnitType.ToString()} to " +
                    $"{targetUnit.UnitType.ToString()} is not possible.");
                }

                if (sourceUnit.ConversionTable.ContainsKey(targetUnit.Name))
                {
                    return Value * sourceUnit.ConversionTable[targetUnit.Name];
                }
                throw new ConversionNotFoundException($"Conversion from {sourceUnit.Name.ToUpperInvariant()} to " +
                    $"{targetUnit.Name.ToUpperInvariant()} wasn't found.");

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
            //TODO: Smart lookup
           Unit foundUnit = Units.Find(x => x.Name == unit || x.Symbol == unit);
            if(foundUnit.UnitType == UnitTypes.None)
            {
                throw new UnitNotFoundException($"{unit} was not found in units table.");
            }
            return foundUnit;
        }
    }
}
