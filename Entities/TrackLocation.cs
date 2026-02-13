namespace weatherapp.Entities;

//Entity is used to track the preferred user weather locations
public class TrackLocation: BaseEntity
{
	public string UserId  { get; set; }
	public Guid LocationId { get; set; }
	public bool isFavorite { get; set; } = false;
	public string? DisplayName { get; set; }
	
	public Location Location { get; set; }
}