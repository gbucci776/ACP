using ACP.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ACP.Data.Identity;
using ACP.Models.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found. " +
        "Configure it using .NET User Secrets.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "SuperAdministrator",
        policy => policy.RequireRole(
            RoleNames.SuperAdministrator));

    options.AddPolicy(
        "ClientPortal",
        policy => policy.RequireRole(
            RoleNames.ClientAdministrator,
            RoleNames.ClientUser));

    options.AddPolicy(
        "ConsumerPortal",
        policy => policy.RequireRole(
            RoleNames.Consumer));
});

builder.Services.AddRazorPages();

var app = builder.Build();

await IdentitySeeder.SeedAsync(
    app.Services,
    app.Configuration,
    app.Environment);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();