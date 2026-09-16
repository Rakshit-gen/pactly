namespace Pactly.Api.Security;

public class JwtOptions
{
    public string SigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "pactly";
    public string Audience { get; set; } = "pactly-clients";
    public int ExpiryMinutes { get; set; } = 60 * 24 * 7;
}
