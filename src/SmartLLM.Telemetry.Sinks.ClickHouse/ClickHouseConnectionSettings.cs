namespace SmartLLM.Telemetry.Sinks.ClickHouse;

/// <summary>Parsed ClickHouse HTTP connection settings.</summary>
public sealed class ClickHouseConnectionSettings
{
    public string Host { get; init; } = "localhost";

    public int Port { get; init; } = 8123;

    public string Database { get; init; } = "smartllm_telemetry";

    public string Username { get; init; } = "default";

    public string Password { get; init; } = string.Empty;

    public string ToConnectionString()
    {
        var cs = $"Host={Host};Port={Port};Database={Database};Username={Username}";
        return string.IsNullOrEmpty(Password) ? cs : $"{cs};Password={Password}";
    }

    public Uri BuildRequestUri(string query)
    {
        var builder = new UriBuilder("http", Host, Port, "/")
        {
            Query = $"database={Uri.EscapeDataString(Database)}&query={Uri.EscapeDataString(query)}"
        };
        return builder.Uri;
    }

    public static ClickHouseConnectionSettings Parse(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string is required.", nameof(connectionString));
        }

        var values = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(p => p.Split('=', 2, StringSplitOptions.TrimEntries))
            .Where(p => p.Length == 2)
            .ToDictionary(p => p[0], p => p[1], StringComparer.OrdinalIgnoreCase);

        var host = values.GetValueOrDefault("Host") ?? "localhost";
        var port = int.Parse(values.GetValueOrDefault("Port") ?? "8123");

        if (host.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || host.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(host);
            host = uri.Host;
            port = uri.Port > 0 ? uri.Port : port;
        }

        return new ClickHouseConnectionSettings
        {
            Host = host,
            Port = port,
            Database = values.GetValueOrDefault("Database") ?? "smartllm_telemetry",
            Username = values.GetValueOrDefault("Username") ?? values.GetValueOrDefault("User") ?? "default",
            Password = values.GetValueOrDefault("Password") ?? string.Empty
        };
    }
}
