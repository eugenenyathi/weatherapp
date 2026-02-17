namespace weatherapp.Entities;

public class Location: BaseEntity
{
	public string Name { get; set; }
	public decimal Latitude { get; set; }
	public decimal Longitude { get; set; }
	public string Country { get; set; }

	public virtual ICollection<DayWeather> DailyWeathers { get; set; } = new HashSet<DayWeather>();
	public virtual ICollection<HourWeather> HourlyWeathers { get; set; } = new HashSet<HourWeather>();
	public virtual ICollection<LocationJob> LocationJobs { get; set; } = new HashSet<LocationJob>();
	public virtual ICollection<LocationSyncSchedule> LocationSyncSchedules { get; set; } = new HashSet<LocationSyncSchedule>();
}
