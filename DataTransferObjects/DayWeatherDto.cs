namespace weatherapp.DataTransferObjects;

public class DayWeatherDto
{
    public DateOnly Date { get; set; }
    public decimal MinTemp { get; set; }
    public decimal MaxTemp { get; set; }
    public int Humidity { get; set; }
}
