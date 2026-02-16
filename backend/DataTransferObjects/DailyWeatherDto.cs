using weatherapp.Enums;

namespace weatherapp.DataTransferObjects;

public class DailyWeatherDto
{
	public string Location { get; set; }
	public Unit Unit { get; set; }
	public List<DayWeatherDto> DayWeathers { get; set; }
}