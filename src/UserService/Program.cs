using System.Text;
using System.Text.Json;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Shared.Behaviors;
using Shared.Configuration;
using Shared.Messaging;
using UserService.Data;
using UserService.Repositories;
using UserService.Services;
using UserService.Workers;


var builder = WebApplication.CreateBuilder(args);

// Konfiguracja Database Context
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "public");
            // Retry logic - automatyczne ponawianie połączenia przy przejściowych błędach
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        }
    )
);
// Redis cache

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "Helpdesk_";
});


// Dependency Injection - Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Dependency Injection - Services
builder.Services.AddScoped<IUserService, UserServiceImpl>(); 
builder.Services.Decorate<IUserService,CachedUserService>();


// Messaging Settings
builder.Services.Configure<Shared.Configuration.MessagingSettings>(
    builder.Configuration.GetSection("MessagingSettings"));

// RabbitMQ Consumer for UserRegisteredEvent - DEPRECATED, now using massTransit for services communicationk
// builder.Services.AddSingleton<IMessageConsumer, RabbitMqConsumer>();
// builder.Services.AddHostedService<UserEventConsumer>();


var messagingSettings = builder.Configuration.GetSection("MessagingSettings").Get<MessagingSettings>();

builder.Services.AddMassTransit( x =>
{
    x.AddConsumer<UserService.Consumers.UserRegisteredConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        if(messagingSettings != null)
        {
            cfg.Host(messagingSettings.HostName, messagingSettings.VirtualHost, h =>
            {
                h.Username(messagingSettings.UserName);
                h.Password(messagingSettings.Password);
            });

        }
        
       
        cfg.ReceiveEndpoint("user-service-user-registered", e =>
        {
            e.ConfigureConsumer<UserService.Consumers.UserRegisteredConsumer>(context);
        });
    });
});


var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddMediatR(cfg =>{
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
});
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "User Service API", 
        Version = "v1",
        Description = "API do zarządzania użytkownikami w systemie Helpdesk"
    });
    
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

// Register RabbitMQ Message Consumer DEPRECATED
// builder.Services.AddSingleton<Shared.Messaging.IMessageConsumer, Shared.Messaging.RabbitMqConsumer>();

// // Register BackgroundService for consuming RabbitMQ events
// builder.Services.AddHostedService<UserService.Workers.UserEventConsumer>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    
    try
    {
        dbContext.Database.Migrate();
        app.Logger.LogInformation("Database migration completed successfully");
        
        // Seed testowych użytkowników
        await UserService.SeedData.InitializeAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Database migration failed or not needed");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");

// HTTPS Redirection - nie potrzebne (ALB robi SSL termination w AWS)
// app.UseHttpsRedirection();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Controllers
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
            service = "UserService",
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

// Database Migration (automatyczne przy starcie w Development)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    
    try
    {
        // Próba migracji
        dbContext.Database.Migrate();
        app.Logger.LogInformation("Database migration completed successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Database migration failed or not needed");
    }
}

app.Logger.LogInformation("UserService started successfully");

app.Run();
