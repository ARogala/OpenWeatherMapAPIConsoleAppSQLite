
namespace OpenWeatherMap
{
    internal class SavedLocations
    {
        private string _city;
        private string _stateCode;
        private string _countryCode;

        public SavedLocations(string city = "", string stateCode = "", string countryCode = "")
        {
            _city = city;
            _stateCode = stateCode;
            _countryCode = countryCode;
        }
        public string City 
        { 
            get { return _city; }
            set { _city = value; }
        }

        public string StateCode
        {
            get { return _stateCode; }
            set { _stateCode = value; } 
        }

        public string CountryCode
        {
            get { return _countryCode; }
            set { _countryCode = value; }
        }

    }
}
