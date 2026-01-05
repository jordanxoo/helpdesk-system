using Hangfire.Dashboard;
namespace TicketService.Configuration;


public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            return true;
        }
        return httpContext.User.Identity?.IsAuthenticated == true 
        && httpContext.User.IsInRole("Administrator");
    }
}