namespace weatherapp.Exceptions;

// Base class for all application exceptions
public abstract class AppException : Exception
{
	public int StatusCode { get; }

	protected AppException(string message, int statusCode = 400)
		: base(message)
	{
		StatusCode = statusCode;
	}

	protected AppException(string message, int statusCode, Exception innerException)
		: base(message, innerException)
	{
		StatusCode = statusCode;
	}
}

// 404 Not Found
public class NotFoundException(string message) : AppException(message, 404);

// 400 Bad Request
public class BadRequestException(string message) : AppException(message);

// 409 Conflict
public class DuplicateEmailException(string message = "Email already exists")
	: AppException(message, 409);

// 409 Conflict - Location already exists
public class DuplicateLocationException(string message = "Location already exists")
	: AppException(message, 409);

// 409 Conflict - Track location already exists
public class DuplicateTrackLocationException(string message = "Location is already being tracked")
	: AppException(message, 409);

// 429 Too Many Requests
public class RateLimitExceededException(TimeSpan retryAfter) : AppException(
	$"Rate limit exceeded. Please try again after {retryAfter.TotalMinutes:F0} minutes.", 429)
{
	public TimeSpan RetryAfter { get; } = retryAfter;
}

// 403 Forbidden
public class ForbiddenException(string message = "Access denied") : AppException(message, 403);

// 400 Bad Request - Invalid data
public class InvalidDataException(string message) : AppException(message);
