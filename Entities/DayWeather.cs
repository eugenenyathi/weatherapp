namespace weatherapp.Entities;

public class DayWeather: BaseEntity
{
	public decimal MinTemp { get; set; }
	public decimal MaxTemp  { get; set; }
	public decimal Humidity { get; set; }
	public decimal Rain { get; set; }
	public string Summary { get; set; }
}