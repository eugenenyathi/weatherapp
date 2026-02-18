namespace weatherapp.DataTransferObjects;

public class RefreshResultDto
{
	public bool Success { get; set; }
	public string Message { get; set; } = string.Empty;
	public DateTime? LastSyncedAt { get; set; }
	public DateTime? NextRefreshAllowedAt { get; set; }
}
