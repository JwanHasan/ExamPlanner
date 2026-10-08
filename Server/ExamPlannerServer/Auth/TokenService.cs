using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ExamPlannerServer.Auth;

/// <summary>Creates the signed token the browser keeps after logging in.</summary>
public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string CreateToken(string id, string email, string name, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),

            // The React app reads exactly these names (jwtUtils.ts / authApi.ts).
            Claims = new Dictionary<string, object>
            {
                ["sub"] = id,
                ["email"] = email,
                ["name"] = name,
                ["role"] = role
            }
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}