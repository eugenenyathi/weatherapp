namespace weatherapp.DataTransferObjects;

public class TrackLocationDto
{
	public Guid Id { get; set; }
	public bool IsFavorite { get; set; }
	public string DisplayName { get; set; }
}