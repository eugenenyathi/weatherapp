namespace weatherapp.DataTransferObjects;

public class HourWeatherDto
{
    public DateTime DateTime { get; set; }
    public decimal TempMetric { get; set; }
    public decimal TempImperial { get; set; }
    public decimal Humidity { get; set; }
    public LocationDto Location { get; set; }
}
