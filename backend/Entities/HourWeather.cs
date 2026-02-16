namespace weatherapp.Entities;

//To be visited when the base app is finished
public class HourWeather: BaseEntity
{
	public Guid LocationId { get; set; }
	public DateTime DateTime { get; set; }
	public decimal TempMetric  {get; set;}
	public decimal TempImperial  {get; set;}
	public decimal Humidity {get; set;}
	public Location Location { get; set; }
}