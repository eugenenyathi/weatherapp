namespace weatherapp.Services.Interfaces;

public interface IOpenWeatherMapHttpClient
{
	Task<HttpResponseMessage> SendAsync(
		HttpMethod method,
		string endpoint,
		object? body = null,
		CancellationToken ct = default);
}