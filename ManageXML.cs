using Spectre.Console;
using System.Xml.Linq;

namespace OpenWeatherMap
{
    internal static class ManageXML
    {
        private static XDocument apiDoc;
        private static XDocument locationsDoc;

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
            else if (docName == "SavedLocations.xml")
            {
                locationsDoc = xmlDoc;
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

        public static List<SavedLocations> GetSavedLocations()
        {
            List<SavedLocations> locationsList = new List<SavedLocations>();
            try
            {
                IEnumerable<XElement> docLocations = locationsDoc.Descendants("Location");
                
                foreach (XElement loc in docLocations)
                {
                    locationsList.Add(new SavedLocations(loc.Element("City").Value, loc.Element("StateCode").Value, loc.Element("CountryCode").Value));
                }
            }
            catch (Exception e)
            {
                AnsiConsole.WriteLine("Failed to get location from xml");
                AnsiConsole.WriteException(e);
            }
            return locationsList;
        }
       
        public static void ChangeDefaultLocation(ushort index)
        {
            //location of element to change is the passed index
            List<SavedLocations> savedLocationsList = GetSavedLocations();
            SavedLocations locationToMakeFirst = savedLocationsList[index];
            savedLocationsList.RemoveAt(index);
            savedLocationsList.Insert(0, locationToMakeFirst);

            //savedLocationsList is now sorted properly 
            //now just need to XML to match
            SaveNewLocationsList(savedLocationsList);
        }
        public static void SaveLocation(string city, string stateCode, string countryCode)
        {
            try
            {
                IEnumerable<XElement> docLocations = locationsDoc.Descendants("Location");
                //will return 0 when no Location element -- perfect way to index
                int id = docLocations.Count();
                //AnsiConsole.WriteLine(docLocations.Count());
                
                XElement newLocation = new XElement("Location");
                newLocation.SetAttributeValue("id", id);
                newLocation.Add(new XElement("City", $"{city}"));
                newLocation.Add(new XElement("StateCode", $"{stateCode}"));
                newLocation.Add(new XElement("CountryCode", $"{countryCode}"));
                locationsDoc.Element("Locations").Add(newLocation);

                locationsDoc.Save("SavedLocations.xml");
            }
            catch (Exception e)
            {
                AnsiConsole.WriteLine("Failed to set and save input location to xml");
                AnsiConsole.WriteException(e);
            }
        }
        public static void RemoveLocation(ushort index)
        {
            //location of element to change is the passed index
            List<SavedLocations> savedLocationsList = GetSavedLocations();
            savedLocationsList.RemoveAt(index);
            SaveNewLocationsList(savedLocationsList);
        }
        private static void SaveNewLocationsList(List<SavedLocations> savedLocationsList)
        {
            try
            {
                //first clear the old Location elements
                locationsDoc.Descendants("Location").Remove();
               
                //then for each new location make a new Location node
                ushort id = 0;
                foreach (SavedLocations savedLocation in savedLocationsList)
                {
                    XElement newLocation = new XElement("Location");
                    newLocation.SetAttributeValue("id", id);
                    newLocation.Add(new XElement("City", $"{savedLocation.City}"));
                    newLocation.Add(new XElement("StateCode", $"{savedLocation.StateCode}"));
                    newLocation.Add(new XElement("CountryCode", $"{savedLocation.CountryCode}"));
                    locationsDoc.Element("Locations").Add(newLocation);
                    id++;
                }
                //finally save the new doc
                locationsDoc.Save("SavedLocations.xml");
            }
            catch (Exception e)
            {
                AnsiConsole.WriteLine("Failed to set and save new locations list");
                AnsiConsole.WriteException(e);
            }
        }
    }
}
