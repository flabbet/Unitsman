using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnitsmanCore.Converter;
using UnitsmanCore.Exceptions;
using UnitsmanCore.IO;

namespace UnitsmanTests
{
    [TestFixture]
    public class ConversionTests
    {

        List<Unit> Units = new List<Unit>();

        [SetUp]
        public void LoadUnits()
        {
            UnitsLoader loader = new UnitsLoader(Path.Combine("..", "..", "..", "..", "Units"));
            Units = loader.LoadUnits();
        }

        [TestCase(1, "ft", "m", ExpectedResult = 0.3048)]
        [TestCase(1, "m", "ft", ExpectedResult = 3.2808399)]
        public double TestThatConvertsCorrectly(double value, string srcUnit, string targetUnit)
        {
            UnitConverter converter = new UnitConverter(Units, value, srcUnit, targetUnit);          
            return converter.Convert();          
        }

        [TestCase(1, "roomba", "catacumba")]
        public void TestThatConverterThrowsUnitNotFound(double value, string srcUnit, string targetUnit)
        {
            UnitConverter converter = new UnitConverter(Units, value, srcUnit, targetUnit);

            Assert.Throws<UnitNotFoundException>(delegate
            {
                FindConverterUnit(srcUnit, converter);
            });
        }

        private Unit FindConverterUnit(string unit, UnitConverter converter)
        {
            return converter.FindUnit(unit);
        }
    }
}
