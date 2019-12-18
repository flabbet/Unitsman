using CommandLine;
using System;
using System.Collections.Generic;
using UnitsmanCore.CLI;
using UnitsmanCore.Converter;
using UnitsmanCore.Exceptions;
using UnitsmanCore.IO;

namespace UnitsmanCore
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var parsedArgs = Parser.Default.ParseArguments<Options>(args);
                Options options = new Options();
                parsedArgs.WithParsed(x => options = x);

                UnitsLoader loader = new UnitsLoader(@"E:\Git\Unitsman\UnitsmanCore\Units");
                Console.WriteLine("Loading Units...");
                List<Unit> units = loader.LoadUnits();
                Console.WriteLine($"{units.Count} units loaded");

                UnitConverter converter = new UnitConverter(units, options.Unit1Value, options.SourceUnit, options.TargetUnit);
                double convertedValue = converter.Convert();
                Console.WriteLine($"{options.Unit1Value}{options.SourceUnit} = {convertedValue}{options.TargetUnit}");
            }
            catch(ConversionException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
