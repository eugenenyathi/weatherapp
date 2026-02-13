using weatherapp.Enums;

namespace weatherapp.Requests
{
    public class UserPreferenceRequest
    {
        public Unit? PreferredUnit { get; set; }
        public int? RefreshInterval { get; set; }
    }
}