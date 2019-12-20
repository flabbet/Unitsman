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

        [TestCase(1, "ft", "m", 0.3048)]
        [TestCase(1, "m", "ft", 3.2808399)]
        [TestCase(5, "yd", "inch", 180)]
        [TestCase(999, "yd", "nautical mile", 0.493242765)]
        [TestCase(-5, "ft", "yard", -1.66666667)]
        [TestCase(-80, "dam", "m", -800)]
        [TestCase(123, "µm", "inch", 0.00484251969)]
        public void TestThatConvertsCorrectly(double value, string srcUnit, string targetUnit, double expectedResult)
        {
            UnitConverter converter = new UnitConverter(Units, value, srcUnit, targetUnit);
            Assert.That(converter.Convert(), Is.EqualTo(expectedResult).Within(0.01));          
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
