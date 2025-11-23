using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Messaging;
using Testcontainers.PostgreSql;
using TicketService.Data;
namespace TicketService.IntegrationTests;
using Xunit;
using NSubstitute;
using NSubstitute.Core;
using Shared.Models;

public class TicketServiceFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
    .WithImage("postgres:16-alpine")
    .WithDatabase("helpdesk_integration_tests")
    .WithUsername("postgres")
    .WithPassword("postgres")
    .Build();


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<TicketDbContext>));

            services.AddDbContext<TicketDbContext>(options => options.UseNpgsql(_dbContainer.GetConnectionString()));

            var publisherMock = Substitute.For<IMessagePublisher>();
            services.RemoveAll(typeof(IMessagePublisher));
            services.AddSingleton(publisherMock);

            services.AddControllers(options =>
            {
                options.Filters.Add(new FakeUserFilter());
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}