namespace NmuShuttle.Api.Infrastructure;

/// <summary>
/// Exception thrown during check-in operations with a specific HTTP status code.
/// Allows the check-in algorithm to signal distinct failure modes (403, 409, 422).
/// </summary>
public class CheckInException : Exception
{
    public int StatusCode { get; }

    public CheckInException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
