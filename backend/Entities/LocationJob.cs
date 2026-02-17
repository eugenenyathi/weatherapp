namespace weatherapp.Entities;

public class LocationJob : BaseEntity
{
	public Guid LocationId { get; set; }
	public string JobId { get; set; } = string.Empty;
	public DateTime JobCreatedAt { get; set; }
	public string Status { get; set; } = "Pending";

	public virtual Location Location { get; set; } = null!;
}
