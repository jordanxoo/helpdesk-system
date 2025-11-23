using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TicketService.IntegrationTests;


public class FakeUserFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()), 
            new Claim(ClaimTypes.Role, "Administrator"), 
            new Claim(ClaimTypes.Email, "test@integration.com")
        };

        var Identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(Identity);

        context.HttpContext.User = principal;

        await next();
    }
}