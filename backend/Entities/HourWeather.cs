namespace weatherapp.Entities;

public class HourWeather: BaseEntity
{
	public Guid LocationId { get; set; }
	public DateTime DateTime { get; set; }
	public decimal TempMetric  {get; set;}
	public decimal TempImperial  {get; set;}
	public decimal Humidity {get; set;}

	public Location Location { get; set; } = null!;
}