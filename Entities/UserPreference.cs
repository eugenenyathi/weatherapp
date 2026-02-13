using weatherapp.Enums;

namespace weatherapp.Entities;

public class UserPreference: BaseEntity
{
	public string UserId  { get; set; }
	public Unit PreferredUnit { get; set; }
	public int RefreshInterval { get; set; }
}