using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace TicketService.IntegrationTests;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder) : base(options,logger,encoder)
    {}

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "testUser"),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Administrator"),
            new Claim(ClaimTypes.Email, "test@integration.com")
        };

        var identity = new ClaimsIdentity(claims, "test");
        var principal  = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "testScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}