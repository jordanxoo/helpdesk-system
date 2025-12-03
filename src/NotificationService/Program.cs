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

// register masstransit

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TicketCreatedConsumer>();
    x.AddConsumer<UserRegisteredConsumer>();
    x.AddConsumer<UserLoggedInConsumer>();

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
        cfg.ConfigureEndpoints(context);
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

app.MapControllers();

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
