using ACP.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace ACP.Data.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager =
            scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        await CreateRolesAsync(roleManager);

        await CreateDevelopmentAdministratorAsync(
            userManager,
            configuration);

        if (environment.IsDevelopment())
        {
            await CreateDemoAccountsAsync(
                userManager,
                configuration);
        }
    }

    private static async Task CreateRolesAsync(
        RoleManager<IdentityRole> roleManager)
    {
        foreach (var roleName in RoleNames.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(
                new IdentityRole(roleName));

            ThrowIfFailed(
                result,
                $"Unable to create role '{roleName}'");
        }
    }

    private static async Task CreateDevelopmentAdministratorAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var email = configuration["SeedAdmin:Email"];
        var password = configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "SeedAdmin:Password must be configured when " +
                    "the administrator account does not already exist.");
            }

            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName =
                    configuration["SeedAdmin:FirstName"]
                    ?? string.Empty,
                LastName =
                    configuration["SeedAdmin:LastName"]
                    ?? string.Empty,
                IsActive = true,
                CreatedUtc = DateTime.UtcNow
            };

            var createResult =
                await userManager.CreateAsync(user, password);

            ThrowIfFailed(
                createResult,
                "Unable to create the seeded administrator");
        }
        else
        {
            var changed = false;

            if (!user.IsActive)
            {
                user.IsActive = true;
                changed = true;
            }

            if (user.CreatedUtc == default)
            {
                user.CreatedUtc = DateTime.UtcNow;
                changed = true;
            }

            if (changed)
            {
                var updateResult =
                    await userManager.UpdateAsync(user);

                ThrowIfFailed(
                    updateResult,
                    "Unable to update the seeded administrator");
            }
        }

        await EnsureRoleAsync(
            userManager,
            user,
            RoleNames.SuperAdministrator);
    }

    private static async Task CreateDemoAccountsAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        await EnsureDemoUserAsync(
            userManager,
            email:
                configuration["DemoAccounts:Client:Email"],
            password:
                configuration["DemoAccounts:Client:Password"],
            firstName: "Demo",
            lastName: "Client",
            roleName: RoleNames.ClientAdministrator);

        await EnsureDemoUserAsync(
            userManager,
            email:
                configuration["DemoAccounts:Consumer:Email"],
            password:
                configuration["DemoAccounts:Consumer:Password"],
            firstName: "Demo",
            lastName: "Consumer",
            roleName: RoleNames.Consumer);
    }

    private static async Task EnsureDemoUserAsync(
        UserManager<ApplicationUser> userManager,
        string? email,
        string? password,
        string firstName,
        string lastName,
        string roleName)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    $"A password must be configured for demo user '{email}'.");
            }

            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true,
                CreatedUtc = DateTime.UtcNow
            };

            var createResult =
                await userManager.CreateAsync(user, password);

            ThrowIfFailed(
                createResult,
                $"Unable to create demo user '{email}'");
        }
        else
        {
            var changed = false;

            if (!user.IsActive)
            {
                user.IsActive = true;
                changed = true;
            }

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                changed = true;
            }

            if (user.CreatedUtc == default)
            {
                user.CreatedUtc = DateTime.UtcNow;
                changed = true;
            }

            if (changed)
            {
                var updateResult =
                    await userManager.UpdateAsync(user);

                ThrowIfFailed(
                    updateResult,
                    $"Unable to update demo user '{email}'");
            }
        }

        await EnsureRoleAsync(
            userManager,
            user,
            roleName);
    }

    private static async Task EnsureRoleAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user,
        string roleName)
    {
        if (await userManager.IsInRoleAsync(user, roleName))
        {
            return;
        }

        var roleResult =
            await userManager.AddToRoleAsync(user, roleName);

        ThrowIfFailed(
            roleResult,
            $"Unable to assign role '{roleName}' " +
            $"to '{user.Email}'");
    }

    private static void ThrowIfFailed(
        IdentityResult result,
        string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(
            "; ",
            result.Errors.Select(error =>
                $"{error.Code}: {error.Description}"));

        throw new InvalidOperationException(
            $"{message}: {errors}");
    }
}