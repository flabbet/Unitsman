using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            var parsedArgs = Parser.Default.ParseArguments<Options>(args);
            Options options = new Options();
            parsedArgs.WithParsed(options => Run(options))
                .WithNotParsed(errs => HandleParseError(errs));

        }


        private static void Run(Options options)
        {
            try
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                UnitsLoader loader = new UnitsLoader(FindParentDirectory(Directory.GetCurrentDirectory(), "Units"));
                Console.WriteLine("Loading Units...");
                List<Unit> units = loader.LoadUnits();
                Console.WriteLine($"{units.Count} units loaded.");

                UnitConverter converter = new UnitConverter(units, options.Unit1Value, options.SourceUnit, options.TargetUnit);
                double convertedValue = Math.Round(converter.Convert(), Math.Clamp(options.Decimals, 0, 15));
                Console.WriteLine($"{options.Unit1Value}{options.SourceUnit} = {convertedValue}{options.TargetUnit}");
            }
            catch (ConversionException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine(ex.Message + $" No units were loaded. Working directory was {Directory.GetCurrentDirectory()}");
            }
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            if (errs.IsVersion())
            {
                Console.WriteLine("Version Request");
                return;
            }

            if (errs.IsHelp())
            {
                Console.WriteLine("Help Request");
                return;
            }
            Console.WriteLine("Parser Fail");
        }

        private static string FindParentDirectory(string currentPath, string targetDirectory)
        {
            if (Directory.GetParent(currentPath) == null) throw new DirectoryNotFoundException($"Could not find directory {targetDirectory}.");
            string[] directories = Directory.GetDirectories(currentPath);
            List<string> directoryNames = new List<string>();
            directories.ToList().ForEach(x => directoryNames.Add(x.Split(Path.DirectorySeparatorChar).Last()));
            if (directoryNames.Contains(targetDirectory))
            {
                return Path.Combine(currentPath, targetDirectory);
            }
            else
            {
                return FindParentDirectory(Directory.GetParent(currentPath).FullName, targetDirectory);
            }
        }

    }
}
