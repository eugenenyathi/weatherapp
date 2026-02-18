using System.Net;
using System.Text.Json;
using weatherapp.Exceptions;

namespace weatherapp.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await next(context);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Unhandled exception occurred");
			await HandleExceptionAsync(context, ex);
		}
	}

	private static Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		if (context.Response.HasStarted)
			return Task.CompletedTask;

		context.Response.ContentType = "application/json";
		int statusCode = (int)HttpStatusCode.InternalServerError;
		string message = "An unexpected error occurred.";

		if (exception is AppException appEx)
		{
			statusCode = appEx.StatusCode;
			message = appEx.Message;
		}

		if (!string.IsNullOrEmpty(exception.Message))
		{
			message = exception.Message;
		}

		context.Response.StatusCode = statusCode;

		// Add Retry-After header for rate limit exceptions
		if (exception is RateLimitExceededException rateLimitEx)
		{
			var retryAfterSeconds = (int)rateLimitEx.RetryAfter.TotalSeconds;
			context.Response.Headers.Append("Retry-After", retryAfterSeconds.ToString());
		}

		var result = JsonSerializer.Serialize(new
		{
			error = message,
			statusCode
		}, new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = true
		});

		return context.Response.WriteAsync(result);
	}
}