using System.Text;
using System.Text.Json;
using weatherapp.Services.Interfaces;

namespace weatherapp.Services;

public class OpenWeatherMapHttpClient(HttpClient httpClient) : IOpenWeatherMapHttpClient
{
	public async Task<HttpResponseMessage> SendAsync(
		HttpMethod method,
		string endpoint,
		object? body = null,
		CancellationToken ct = default)
	{
		var request = new HttpRequestMessage(method, endpoint);

		if (body != null)
		{
			request.Content = new StringContent(
				JsonSerializer.Serialize(body),
				Encoding.UTF8,
				"application/json");
		}

		var response = await httpClient.SendAsync(request, ct);
		response.EnsureSuccessStatusCode();

		return response;
	}
}
