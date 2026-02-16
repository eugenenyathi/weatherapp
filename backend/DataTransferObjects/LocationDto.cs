namespace weatherapp.DataTransferObjects;

public class LocationDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public decimal Latitude { get; set; }
	public decimal Longitude { get; set; }
	public string Country { get; set; } = string.Empty;
}