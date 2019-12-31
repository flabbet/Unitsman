using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnitsmanCore.Converter;
using UnitsmanCore.IO;

namespace UnitsmanTests
{
    [TestFixture]
    public class IOTests
    {
        [Test]
        public void TestThatLoaderDeserializesAllFiles()
        {
            var path = UnitsLoader.FindParentDirectory(Directory.GetCurrentDirectory(), "TestFiles");
            UnitsLoader loader = new UnitsLoader(path);
            List<Unit> units = loader.LoadUnits();
            Assert.AreEqual("meter", units[0].Name);
            Assert.AreEqual("m", units[0].Symbol);
            Assert.AreEqual("length", units[0].UnitType);
            Assert.AreEqual(true, units[0].UsesSIPrefixes);
            Assert.AreEqual(3.2, units[0].ConversionTable["foot"]);
            Assert.AreEqual(39.37, units[0].ConversionTable["inch"]);
        }
    }
}
