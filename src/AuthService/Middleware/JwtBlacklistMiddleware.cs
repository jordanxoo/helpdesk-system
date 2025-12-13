using System.IdentityModel.Tokens.Jwt;
using AuthService.Services;

namespace AuthService.Middleware;


public class JwtBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtBlacklistMiddleware> _logger;


    public JwtBlacklistMiddleware(RequestDelegate requestDelegate, ILogger<JwtBlacklistMiddleware> logger)
    {
        _next = requestDelegate;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ISessionService sessionService)
    {
        if(context.User.Identity?.IsAuthenticated == true)
        {
            var sessionId = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if(!string.IsNullOrEmpty(sessionId))
            {
                var isRevoked = await sessionService.IsSessionRevokedAsync(sessionId);

                if(isRevoked)
                {
                    _logger.LogWarning("Access Denied - token is blacklisted. SessionId {sessionID}",sessionId);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "Token has been revoked. Please login again"
                    });
                    return;
                }
            }
        }
        await _next(context);
    }

}
