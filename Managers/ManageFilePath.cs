using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenWeatherMap.Managers
{
   
    internal static class ManageFilePath
    {
        private static string appDirectory = Directory.GetCurrentDirectory();
        private static string dataDirectory = Directory.GetDirectories(appDirectory, "Data").First();

        public static string GetPath(string fileName)
        {
            return Directory.GetFiles(dataDirectory, $"{fileName}").First();
        }
    }
}
