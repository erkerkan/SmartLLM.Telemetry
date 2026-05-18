namespace SmartLLM.Telemetry.Sinks.ClickHouse;

/// <summary>Raised when ClickHouse rejects an HTTP insert.</summary>
public sealed class ClickHouseInsertException : Exception
{
    public ClickHouseInsertException(string message, int statusCode, string responseBody)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public int StatusCode { get; }

    public string ResponseBody { get; }
}
