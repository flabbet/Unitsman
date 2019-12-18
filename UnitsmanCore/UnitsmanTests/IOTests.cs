using NUnit.Framework;
using System.Collections.Generic;
using UnitsmanCore.Converter;
using UnitsmanCore.IO;

namespace UnitsmanTests
{
    public class IOTests
    {
        [Test]
        public void TestThatLoaderDeserializesAllFiles()
        {
            UnitsLoader loader = new UnitsLoader("TestFiles");
            List<Unit> units = loader.LoadUnits();
            Assert.AreEqual("meter", units[0].Name);
            Assert.AreEqual("m", units[0].Symbol);
            Assert.AreEqual(UnitTypes.Length, units[0].UnitType);
            Assert.AreEqual(true, units[0].UsesSIPrefixes);
            Assert.AreEqual(3.2, units[0].ConversionTable["foot"]);
            Assert.AreEqual(39.37, units[0].ConversionTable["inch"]);
        }
    }
}
