using System.IO;
using System.Collections.Generic;
using UnitsmanCore.Converter;
using Newtonsoft.Json;
using System.Linq;

namespace UnitsmanCore.IO
{
    public class UnitsLoader
    {
        public string PathToFolder;

        public UnitsLoader(string pathToFolder)
        {
            PathToFolder = pathToFolder;
        }

        public List<Unit> LoadUnits()
        {
            List<Unit> finalList = new List<Unit>();
            string[] filesInDir = Directory.GetFiles(PathToFolder,"*.json", SearchOption.AllDirectories);
            for(int i = 0; i < filesInDir.Length; i++)
            {
                string rawJson = File.ReadAllText(filesInDir[i]);
                finalList.Add(JsonConvert.DeserializeObject<Unit>(rawJson));
            }
            return finalList;
        }

        public static string FindParentDirectory(string currentPath, string targetDirectory)
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
