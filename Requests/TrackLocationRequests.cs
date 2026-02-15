namespace weatherapp.Requests;

public class CreateTrackLocationRequest
{
	public Guid LocationId { get; set; }
	public bool? IsFavorite { get; set; }
	public string? DisplayName { get; set; }
}

public class UpdateTrackLocationRequest
{
	public bool? IsFavorite { get; set; }
	public string? DisplayName { get; set; }
}