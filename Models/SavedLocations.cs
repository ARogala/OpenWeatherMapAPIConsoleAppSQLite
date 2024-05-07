namespace OpenWeatherMap.Models
{
    internal class SavedLocations
    {
        private string _city;
        private string _stateCode;
        private string _countryCode;
        private int _locationId;
        private int _isDefault;
        private float? _latitude;
        private float? _longitude;

        //added empty constructor for GetLocationAtId in ManageSQL
        public SavedLocations()
        {

        }

        public SavedLocations(string city, string stateCode, string countryCode, int locationID, int isDefault, float? latitude = null, float? longitude = null)
        {
            _city = city;
            _stateCode = stateCode;
            _countryCode = countryCode;
            _latitude = latitude;
            _longitude = longitude;
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

        public int LocationId
        {
            get { return _locationId; }
            set { _locationId = value; }
        }

        public int IsDefalut
        {
            get { return _isDefault; }
            set { _isDefault = value; }
        }

        public float? Latitude
        {
            get { return _latitude; }
            set { _latitude = value; }
        }

        public float? Longitude
        {
            get { return _longitude; }
            set { _longitude = value; }
        }
    }
}
