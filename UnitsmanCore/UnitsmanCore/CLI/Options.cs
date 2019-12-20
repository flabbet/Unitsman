using CommandLine;

namespace UnitsmanCore.CLI
{
    public class Options
    {
        [Value(0, MetaName = "Value", Required = true, HelpText = "Value you want to convert (ex. 20).")]
        public double Unit1Value {get;set;}

        [Value(1, MetaName = "Source Unit", Required = true, HelpText = "Unit of value (ex. inch, in). Use singular version (foot insead of feets).")]
        public string SourceUnit { get; set; }

        [Value(2, MetaName = "Target Unit", Required = true, HelpText = "Unit you want to convert to (ex. meter, m).")]
        public string TargetUnit { get; set; }

        [Option('d', "decimals", Default = 3, Required = false, HelpText = "Sets how precise output should be.")]
        public int Decimals { get; set; }
    }
}
