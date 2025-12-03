using UserService.Data;
using Shared.Models;

namespace UserService;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<UserDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        // Sprawdź czy użytkownicy już istnieją
        if (context.Users.Any())
        {
            logger.LogInformation("Users already exist in database, skipping seed");
            return;
        }

        logger.LogInformation("Creating seed users...");

        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                Email = "customer@test.com",
                FirstName = "Jan",
                LastName = "Kowalski",
                PhoneNumber = "+48 123 456 789",
                Role = UserRole.Customer,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                Email = "agent@test.com",
                FirstName = "Anna",
                LastName = "Nowak",
                PhoneNumber = "+48 987 654 321",
                Role = UserRole.Agent,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@test.com",
                FirstName = "Piotr",
                LastName = "Wiśniewski",
                PhoneNumber = "+48 555 666 777",
                Role = UserRole.Administrator,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        logger.LogInformation("Created {Count} seed users", users.Count);
    }
}
