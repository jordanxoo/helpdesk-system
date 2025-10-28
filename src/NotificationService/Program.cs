using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NotificationService.Configuration;
using NotificationService.HealthChecks;
using NotificationService.Services;
using NotificationService.Workers;
using Shared.Configuration;
using Shared.Messaging;

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

// Register RabbitMQ Consumer
builder.Services.AddSingleton<IMessageConsumer, RabbitMqConsumer>();

// Register Background Worker
builder.Services.AddHostedService<NotificationWorker>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Notification Service API", Version = "v1" });
});

// Health Checks - RabbitMQ connection
builder.Services.AddHealthChecks()
    .AddCheck<RabbitMqHealthCheck>("rabbitmq");

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
