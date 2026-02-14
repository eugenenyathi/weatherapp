namespace weatherapp.DataTransferObjects;

public class OpenWeatherDailyResponse
{
	public List<DailyForecastDto> Daily { get; set; } = new();
}

public class DailyForecastDto
{
	public long Dt { get; set; } 
	public decimal Humidity { get; set; }
	public decimal Rain { get; set; } 
	public string Summary { get; set; }
	public TempDto Temp { get; set; }
}


public class TempDto
{
	public decimal Min { get; set; }
	public decimal Max { get; set; }
}
