using CommandLine;

namespace UnitsmanCore.CLI
{
    public class Options
    {
        [Value(0)]
        public double Unit1Value {get;set;}

        [Value(1)]
        public string Unit1Type { get; set; }

        [Value(2)]
        public string Unit2Type { get; set; }
    }
}
