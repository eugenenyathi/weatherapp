using weatherapp.Enums;

namespace weatherapp.DataTransferObjects;

public class UserPreferenceDto
{
	public Guid Id { get; set; }
	public Unit PreferredUnit { get; set; }
	public int RefreshInterval { get; set; }
}