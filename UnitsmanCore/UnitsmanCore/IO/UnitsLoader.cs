using System.IO;
using System.Collections.Generic;
using UnitsmanCore.Converter;
using Newtonsoft.Json;

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
            string[] filesInDir = Directory.GetFiles(PathToFolder, "*.json");
            for(int i = 0; i < filesInDir.Length; i++)
            {
                string rawJson = File.ReadAllText(filesInDir[i]);
                finalList.Add(JsonConvert.DeserializeObject<Unit>(rawJson));
            }
            return finalList;
        }
    }
}
