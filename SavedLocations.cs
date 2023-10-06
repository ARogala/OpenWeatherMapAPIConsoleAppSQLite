
namespace OpenWeatherMap
{
    internal class SavedLocations
    {
        private string _city;
        private string _stateCode;
        private string _countryCode;
        private string _locationId;
        private string _isDefault;

        public SavedLocations(string city = "", string stateCode = "", string countryCode = "", string locationID = "", string isDefault = "")
        {
            _city = city;
            _stateCode = stateCode;
            _countryCode = countryCode;
            _locationId = locationID;
            _isDefault = isDefault;
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

        public string LocationId
        {
            get { return _locationId; }
            set { _locationId = value; }  
        }

        public string IsDefalut
        {
            get { return _isDefault; }
            set { _isDefault = value; }
        }
    }
}
