namespace weatherapp.DataTransferObjects;

public class OpenWeatherHourlyResponse
{
	public List<HourlyForecastDto> Hourly { get; set; } = new();
}

public class HourlyForecastDto
{
	public long Dt { get; set; } 
	public decimal Humidity { get; set; }
	public decimal Temp { get; set; }
}

