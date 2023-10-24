using Spectre.Console;
using System.Xml.Linq;

namespace OpenWeatherMap
{
    internal static class ManageXML
    {
        private static XDocument apiDoc;

        public static void LoadXML(string docName)
        {
            XDocument xmlDoc = new XDocument();
            try
            {
                xmlDoc = XDocument.Load(docName);
            }
            catch (Exception e)
            {
                AnsiConsole.WriteLine("xml document failed to load " + docName);
                AnsiConsole.WriteException(e);
            }
            if (docName == "APIKEY.xml")
            {
                apiDoc = xmlDoc;
            }
            
        }

        public static string GetAPIKey()
        {
            string docApiKey = string.Empty;
            try
            {
                docApiKey = apiDoc.Element("ApiKey").Value;
            }
            catch (Exception e)
            {
                AnsiConsole.WriteLine("Failed to get api key from xml");
                AnsiConsole.WriteException(e);
            }
            return docApiKey;
        }

        public static void SetAPIKey(string inputApiKey)
        {

            try
            {
                apiDoc.Element("ApiKey").Value = inputApiKey;
                apiDoc.Save("APIKEY.xml");
            }
            catch (Exception e)
            {
                AnsiConsole.WriteLine("Failed to set and save input api key to xml");
                AnsiConsole.WriteException(e);
            }
        }
        
    }
}
