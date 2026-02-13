using weatherapp.Enums;

namespace weatherapp.DataTransferObjects
{
    public class UserPreferenceDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Unit PreferredUnit { get; set; }
        public int RefreshInterval { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}