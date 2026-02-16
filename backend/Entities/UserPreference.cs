using weatherapp.Enums;

namespace weatherapp.Entities;

public class UserPreference: BaseEntity
{
	public Guid UserId  { get; set; }
	public Unit PreferredUnit { get; set; }
	public int RefreshInterval { get; set; }
	
	public User User { get; set; }
}