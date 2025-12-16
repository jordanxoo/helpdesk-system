using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NotificationService.Configuration;
using NotificationService.Services;
//using NotificationService.Workers;
using Shared.Configuration;
//using Shared.Messaging;
using NotificationService.Consumers;
using MassTransit;
using Shared.Events;
using NotificationService.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MassTransit.Logging;
using Shared.HttpClients;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Configure Email Settings
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Configure Messaging Settings (RabbitMQ)
builder.Services.Configure<MessagingSettings>(
    builder.Configuration.GetSection("MessagingSettings"));

// Register Services
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<ISmsService, SmsService>();

builder.Services.AddSingleton<ISignalRNotificationService,SignalRNotificationService>();

builder.Services.AddHttpClient<IUserServiceClient,UserServiceClient>(client =>
{
    var userServiceUrl = builder.Configuration["ServiceUrls:UserService"]
    ?? "http://helpdesk-users:8080";
    client.BaseAddress = new Uri(userServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});


builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "Helpdesk_";
});

builder.Services.AddScoped<INotificationQueueService, RedisNotificationQueueService>();

// signal r z redis backplane
builder.Services.AddSignalR()
.AddStackExchangeRedis(
builder.Configuration.GetConnectionString("Redis") ?? "helpdesk-redis:6379",
options =>
{
    options.Configuration.ChannelPrefix = StackExchange.Redis.RedisChannel.Literal("helpdesk-signalr");
});


// cors dla signalR

builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",
            "http://localhost:5173",
            "http://localhost:5174") //react/vite frontend
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// jwt authentication

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    var jwtSecret = builder.Configuration["JwtSettings:Secretkey"];
    if(string.IsNullOrEmpty(jwtSecret))
    {
        throw new InvalidOperationException("JWT SecretKey is not configured");
    }
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience =true,
        ValidateLifetime  =true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if(!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});



// register masstransit

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserRegisteredConsumer>();
    x.AddConsumer<UserLoggedInConsumer>();

    x.AddConsumer<TicketCreatedConsumer>();
    x.AddConsumer<TicketAssignedConsumer>();
    x.AddConsumer<TicketStatusChangedConsumer>();
    x.AddConsumer<TicketCommentAddedConsumer>();

    x.UsingRabbitMq((context,cfg) =>
    {
        var messagingSettings = builder.Configuration
            .GetSection("MessagingSettings")
            .Get<MessagingSettings>();

        if (messagingSettings == null)
        {
            throw new InvalidOperationException("MessagingSettings not configured");
        }

        cfg.Host(messagingSettings.HostName, "/", h =>
        {
            h.Username(messagingSettings.UserName);
            h.Password(messagingSettings.Password);
        });
        
        // Konfiguracja endpointów z unikalnymi nazwami kolejek dla NotificationService
        cfg.ReceiveEndpoint("notification-service-ticket-created", e =>
        {
            e.ConfigureConsumer<TicketCreatedConsumer>(context);
        });
        
        cfg.ReceiveEndpoint("notification-service-user-registered", e =>
        {
            e.ConfigureConsumer<UserRegisteredConsumer>(context);
        });
        
        cfg.ReceiveEndpoint("notification-service-user-logged-in", e =>
        {
            e.ConfigureConsumer<UserLoggedInConsumer>(context);
        });

        cfg.ReceiveEndpoint("notification-service-ticket-assigned", e => {
            e.ConfigureConsumer<TicketAssignedConsumer>(context);
        });
        cfg.ReceiveEndpoint("notification-service-ticket-status-changed",e =>
        {
            e.ConfigureConsumer<TicketStatusChangedConsumer>(context);
        });
        cfg.ReceiveEndpoint("notification-service-comment-added", e =>{
            e.ConfigureConsumer<TicketCommentAddedConsumer>(context);
        });
    });
});





builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Notification Service API", Version = "v1" });
});

// Health Checks - Basic health check
builder.Services.AddHealthChecks();

var app = builder.Build();

// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Service API v1");
});

app.UseCors("SignalRPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//mapowanie signalRHub

app.MapHub<NotificationHub>("/hubs/notifications");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context,report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(
            new
            {
                status = report.Status.ToString(),
                service = "NotificationService",
                timestamp = DateTime.UtcNow,
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    duration = e.Value.Duration.TotalMilliseconds
                }) 
            });
            await context.Response.WriteAsync(result);
    }
});



// Health Check endpoint z custom JSON response
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            service = "NotificationService",
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds
            })
        });
        
        await context.Response.WriteAsync(result);
    }
});

app.Logger.LogInformation("NotificationService starting...");

app.Run();
