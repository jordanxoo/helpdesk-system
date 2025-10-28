var builder = WebApplication.CreateBuilder(args);

// TODO: Konfiguracja serwisów

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// TODO: Konfiguracja middleware

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS Redirection - nie potrzebne (ALB robi SSL termination w AWS)
// app.UseHttpsRedirection(); // USUNIĘTE

app.MapControllers();

app.Run();
