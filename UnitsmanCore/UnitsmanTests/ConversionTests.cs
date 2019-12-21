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
        [TestCase(999, "nautical mile", "yd", 2023346.46)]
        [TestCase(-5, "ft", "yard", -1.66666667)]
        [TestCase(-80, "dam", "m", -800)]
        [TestCase(123, "µm", "inch", 0.00484251969)]
        [TestCase(2, "m/s", "mm/s", 2000)]
        [TestCase(0.2, "km/h", "m/s", 0.0555555556)]
        [TestCase(50, "yd/h", "m/s", 0.0127)]
        [TestCase(0.2, "Mm/h", "m/s", 55.5555556)]
        [TestCase(214412, "joules per second", "watt", 214412)]
        [TestCase(214412, "watt", "joules per second", 214412)]
        public void TestThatConvertsCorrectly(double value, string srcUnit, string targetUnit, double expectedResult)
        {
            UnitConverter converter = new UnitConverter(Units, value, srcUnit, targetUnit);
            Assert.That(converter.Convert(), Is.EqualTo(expectedResult).Within(0.01));          
        }

        [TestCase("roomba")]
        public void TestThatFinderThrowsUnitNotFound(string srcUnit)
        {
            UnitFinder finder = new UnitFinder(Units);

            Assert.Throws<UnitNotFoundException>(delegate
            {
                FindConverterUnit(srcUnit, finder);
            });
        }

        private Unit FindConverterUnit(string unit, UnitFinder converter)
        {
            return converter.FindUnit(unit);
        }
    }
}
