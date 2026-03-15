using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Friendout.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using FluentAssertions;

namespace Friendout.Test;

public class JwtServiceTests
{
    [Test]
    public void GenerateJwt_WithValidConfiguration_ReturnsValidToken()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            // HS256 requires a key of at least 256 bits (32 bytes)
            ["Jwt:Key"] = "this_is_a_test_key_1234567890_ABCDEF",
            ["Jwt:Issuer"] = "friendout-tests",
            ["Jwt:Audience"] = "friendout-audience"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var service = new JwtService(configuration);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-id"),
            new(ClaimTypes.Name, "Test User")
        };

        // Act
        var tokenString = service.GenerateJwt(claims);

        // Assert
        tokenString.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        token.Issuer.Should().Be("friendout-tests");
        token.Audiences.Should().Contain("friendout-audience");
        token.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "user-id");
    }
}