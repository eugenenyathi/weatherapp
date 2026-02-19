namespace weatherapp.DataTransferObjects;

public class HourWeatherDto
{
    public DateTime DateTime { get; set; }
    public decimal Temp { get; set; }
    public decimal Humidity { get; set; }
}
