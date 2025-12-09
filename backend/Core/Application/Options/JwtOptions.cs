namespace Core.Application.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public double ExpireHours { get; set; } = 1;
    public double RefreshTokenExpireHours { get; set; } = 24;
}