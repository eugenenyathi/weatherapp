namespace weatherapp.Entities;

public class DayWeather: BaseEntity
{
	public Guid LocationId { get; set; }
	public DateOnly Date { get; set; }
	public DateTime TimeOfForecast { get; set; }
	// Temperature fields
	public decimal MinTempMetric { get; set; }
	public decimal MaxTempMetric { get; set; }
	public decimal MinTempImperial { get; set; }
	public decimal MaxTempImperial { get; set; }
	public decimal Humidity { get; set; }
	public decimal? Rain { get; set; }
	public string Summary { get; set; }
	public Location Location { get; set; }
}