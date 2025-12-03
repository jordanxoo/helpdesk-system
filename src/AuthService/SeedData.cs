using Microsoft.AspNetCore.Identity;
using AuthService.Data;

namespace AuthService;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Utwórz role jeśli nie istnieją
        string[] roles = { "Customer", "Agent", "Administrator" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Utwórz testowego Customera
        await CreateUserIfNotExistsAsync(userManager, 
            "customer@test.com", 
            "Customer123!", 
            "Customer");

        // Utwórz testowego Agenta
        await CreateUserIfNotExistsAsync(userManager, 
            "agent@test.com", 
            "Agent123!", 
            "Agent");

        // Utwórz testowego Administratora
        await CreateUserIfNotExistsAsync(userManager, 
            "admin@test.com", 
            "Admin123!", 
            "Administrator");
    }

    private static async Task CreateUserIfNotExistsAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string role)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
                Console.WriteLine($"Created test user: {email} with role {role}");
            }
            else
            {
                Console.WriteLine($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            Console.WriteLine($"User {email} already exists");
        }
    }
}
