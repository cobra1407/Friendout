using FluentAssertions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Friendout.Infrastructure.Services;

namespace Friendout.Test;

public class JwtServiceConfigurationTests
{
    [Test]
    public void GenerateJwt_WithValidConfiguration_ReturnsSignedToken()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "this-is-a-long-enough-test-secret-key-123456",
                ["Jwt:Issuer"] = "friendout-tests",
                ["Jwt:Audience"] = "friendout-clients"
            })
            .Build();

        var service = new JwtService(config);

        var token = service.GenerateJwt(new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-1"),
            new(ClaimTypes.Name, "Alice")
        });

        token.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        jwt.Issuer.Should().Be("friendout-tests");
        jwt.Audiences.Should().Contain("friendout-clients");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "user-1");
    }

    [Test]
    public void GenerateJwt_WhenConfigurationMissing_ThrowsInvalidOperationException()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var service = new JwtService(config);

        var action = () => service.GenerateJwt(new List<Claim>());

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("JWT configuration values (Key, Issuer, Audience) must be provided.");
    }
}


