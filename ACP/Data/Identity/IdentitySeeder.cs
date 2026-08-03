using ACP.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace ACP.Data.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
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

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(error =>
                        $"{error.Code}: {error.Description}"));

                throw new InvalidOperationException(
                    $"Unable to create role '{roleName}': {errors}");
            }
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
                    "the seeded administrator does not already exist.");
            }

            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = configuration["SeedAdmin:FirstName"]
                    ?? string.Empty,
                LastName = configuration["SeedAdmin:LastName"]
                    ?? string.Empty,
                IsActive = true,
                CreatedUtc = DateTime.UtcNow
            };

            var creationResult =
                await userManager.CreateAsync(user, password);

            if (!creationResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    creationResult.Errors.Select(error =>
                        $"{error.Code}: {error.Description}"));

                throw new InvalidOperationException(
                    $"Unable to create the seeded administrator: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(
                user,
                RoleNames.SuperAdministrator))
        {
            var roleResult = await userManager.AddToRoleAsync(
                user,
                RoleNames.SuperAdministrator);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(error =>
                        $"{error.Code}: {error.Description}"));

                throw new InvalidOperationException(
                    $"Unable to assign the administrator role: {errors}");
            }
        }
    }
}