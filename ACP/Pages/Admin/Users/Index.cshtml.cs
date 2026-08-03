using ACP.Data.Identity;
using ACP.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACP.Pages.Admin.Users;

[Authorize(Policy = "SuperAdministrator")]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public IReadOnlyList<UserListItem> Users { get; private set; } =
        Array.Empty<UserListItem>();

    public async Task OnGetAsync()
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .Include(user => user.ClientOrganization)
            .OrderBy(user => user.ClientOrganization!.CompanyName)
            .ThenBy(user => user.Email)
            .ToListAsync();

        var userItems = new List<UserListItem>(users.Count);

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var isClientUser =
                roles.Contains(RoleNames.ClientAdministrator)
                || roles.Contains(RoleNames.ClientUser);

            userItems.Add(new UserListItem
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive,
                CreatedUtc = user.CreatedUtc,
                LastLoginUtc = user.LastLoginUtc,
                Roles = roles
                    .OrderBy(role => role)
                    .ToArray(),

                ClientOrganizationId =
                    user.ClientOrganizationId,

                CompanyName =
                    user.ClientOrganization?.CompanyName,

                IsClientUser = isClientUser
            });
        }

        Users = userItems;
    }
}