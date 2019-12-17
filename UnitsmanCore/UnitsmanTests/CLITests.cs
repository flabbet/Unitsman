using CommandLine;
using NUnit.Framework;
using UnitsmanCore.CLI;

namespace UnitsmanTests
{
    public class Tests
    {
        [Test]
        public void TestThatArgsAreParsingCorrectly()
        {
            string[] args = new string[] { "15", "km", "m" };
            var parsedArgs = Parser.Default.ParseArguments<Options>(args);
            Options opts = new Options();
            parsedArgs.WithParsed(x => opts = x);
            Assert.AreEqual(args[0], opts.Unit1Value.ToString());
            Assert.AreEqual(args[1], opts.Unit1Type);
            Assert.AreEqual(args[2], opts.Unit2Type);
        }
    }
}