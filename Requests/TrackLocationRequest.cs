namespace weatherapp.Requests;

public class TrackLocationRequest
{
	public Guid LocationId { get; set; }
	public bool? IsFavorite { get; set; }
	public string? DisplayName { get; set; }
}