using SmartLLM.Telemetry.Security;
using Xunit;

namespace SmartLLM.Telemetry.Security.Tests;

public class PiiRedactorTests
{
    [Fact]
    public void Redact_masks_email()
    {
        var redactor = new PiiRedactor(Microsoft.Extensions.Options.Options.Create(new PiiRedactionOptions()));
        var result = redactor.Redact("Contact me at user@contoso.com please.");
        Assert.DoesNotContain("user@contoso.com", result);
        Assert.Contains("[REDACTED:email]", result);
    }
}
