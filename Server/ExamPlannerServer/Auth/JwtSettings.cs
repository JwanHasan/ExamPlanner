using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ExamPlannerServer.Auth;

/// <summary>The "Jwt" section of appsettings.</summary>
public sealed class JwtSettings
{
    public const string Section = "Jwt";

    /// <summary>
    /// Signing key, at least 32 characters. Real keys never go in git:
    /// set them with the Jwt__Key environment variable instead.
    /// </summary>
    public string Key { get; set; } = "";

    public string Issuer { get; set; } = "ExamPlanner";
    public string Audience { get; set; } = "ExamPlanner";

    /// <summary>How long a login lasts before the user has to log in again.</summary>
    public int ExpiresHours { get; set; } = 8;

    public SymmetricSecurityKey SigningKey() => new(Encoding.UTF8.GetBytes(Key));

    /// <summary>Stops the API at startup with a clear message, instead of failing on the first login.</summary>
    public void Validate()
    {
        if (Encoding.UTF8.GetByteCount(Key) < 32)
            throw new InvalidOperationException(
                "Jwt:Key is missing or shorter than 32 characters. " +
                "Set it in appsettings.Development.json, or as the Jwt__Key environment variable.");

        if (ExpiresHours <= 0)
            throw new InvalidOperationException("Jwt:ExpiresHours must be greater than 0.");
    }
}
