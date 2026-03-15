using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Friendout.Infrastructure.Services;

/// <summary>
/// Service responsible for generating JSON Web Tokens (JWT) for authenticated users.
/// </summary>
public class JwtService
{
    private readonly IConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtService"/> class.
    /// </summary>
    /// <param name="config">The application configuration used to retrieve JWT settings.</param>
    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Generates a signed JWT token using the provided user claims.
    /// </summary>
    /// <param name="claims">A list of claims to embed inside the token.</param>
    /// <returns>A JWT token serialized as a string.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required JWT configuration values (Key, Issuer, Audience) are missing.
    /// </exception>
    public string GenerateJwt(List<Claim> claims)
    {
        var keyString = _config["Jwt:Key"];
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];

        if (string.IsNullOrEmpty(keyString) || issuer is null || audience is null)
        {
            throw new InvalidOperationException("JWT configuration values (Key, Issuer, Audience) must be provided.");
        }

        var key = Encoding.UTF8.GetBytes(keyString);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            )
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}