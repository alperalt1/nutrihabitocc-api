namespace Nutrihabitocc.Domain.Constants;

public static class Platforms
{
    public const string Web = "web";
    public const string Mobile = "mobile";

    public static bool IsValid(string? platform)
    {
        return string.Equals(platform, Web, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(platform, Mobile, StringComparison.OrdinalIgnoreCase);
    }

    public static string Normalize(string? platform)
    {
        if (string.IsNullOrWhiteSpace(platform))
        {
            return Web;
        }

        return platform.Trim().ToLowerInvariant();
    }
}
