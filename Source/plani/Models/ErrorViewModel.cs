namespace plani.Models;

public class ErrorViewModel
{
	public string RequestId { get; set; }
	public bool ShowRequestId => string.IsNullOrEmpty(RequestId);
	public string ExceptionMessage { get; set; } = string.Empty;
	public string StackTrace { get; set; } = string.Empty;
}
