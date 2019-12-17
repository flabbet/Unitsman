using CommandLine;
using System;
using UnitsmanCore.CLI;

namespace UnitsmanCore
{
    class Program
    {
        static void Main(string[] args)
        {
            var parsedArgs = Parser.Default.ParseArguments<Options>(args);
            
        }

    }
}
