using Hangfire.Dashboard;

namespace AuthService.Configuration;



public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpcontext = context.GetHttpContext();

        if(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            return true;
        }

        return httpcontext.User.Identity?.IsAuthenticated == true 
        && httpcontext.User.IsInRole("Administrator");
    }
}