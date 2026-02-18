namespace weatherapp.Exceptions;

public class RateLimitExceededException : Exception
{
	public TimeSpan RetryAfter { get; }

	public RateLimitExceededException(TimeSpan retryAfter)
		: base($"Rate limit exceeded. Please try again after {retryAfter.TotalMinutes:F0} minutes.")
	{
		RetryAfter = retryAfter;
	}
}
