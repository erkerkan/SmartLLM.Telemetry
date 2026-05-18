using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using SmartLLM.Telemetry.Core;

namespace SmartLLM.Telemetry.OpenTelemetry;

internal sealed class SmartLLMResourceConfiguration : IConfigureOptions<ResourceBuilder>
{
    private readonly IOptions<SmartLLMTelemetryOptions> _options;

    public SmartLLMResourceConfiguration(IOptions<SmartLLMTelemetryOptions> options)
    {
        _options = options;
    }

    public void Configure(ResourceBuilder builder)
    {
        builder.AddService(serviceName: _options.Value.ServiceName);
    }
}
