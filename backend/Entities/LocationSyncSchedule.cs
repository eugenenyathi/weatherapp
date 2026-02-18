using System.ComponentModel.DataAnnotations.Schema;

namespace weatherapp.Entities;

public class LocationSyncSchedule : BaseEntity
{
	public Guid UserId { get; set; }
	public Guid LocationId { get; set; }
	public DateTime LastSyncAt { get; set; }
	public DateTime NextSyncAt { get; set; }
	public string RecurringJobId { get; set; } = string.Empty;
	public virtual User User { get; set; } = null!;
	
	public virtual Location Location { get; set; } = null!;
}
