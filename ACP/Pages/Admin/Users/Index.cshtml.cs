using System.Security.Claims;
using ACP.Data.Identity;
using ACP.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACP.Pages.Admin.Users;

[Authorize(Policy = "SuperAdministrator")]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IndexModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public IReadOnlyList<UserListItem> Users { get; private set; } =
        Array.Empty<UserListItem>();

    [TempData]
    public string? StatusMessage { get; set; }

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

    public async Task<IActionResult> OnPostImpersonateAsync(
        string userId)
    {
        var administratorId =
            _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(administratorId))
        {
            return Challenge();
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            StatusMessage =
                "No client account was selected.";

            return RedirectToPage();
        }

        var selectedUser = await _userManager.Users
            .Include(user => user.ClientOrganization)
            .FirstOrDefaultAsync(user => user.Id == userId);

        if (selectedUser is null)
        {
            StatusMessage =
                "The selected user could not be found.";

            return RedirectToPage();
        }

        if (selectedUser.Id == administratorId)
        {
            StatusMessage =
                "You cannot impersonate your own administrator account.";

            return RedirectToPage();
        }

        if (!selectedUser.IsActive)
        {
            StatusMessage =
                "You cannot view the portal for a disabled account.";

            return RedirectToPage();
        }

        if (!selectedUser.ClientOrganizationId.HasValue)
        {
            StatusMessage =
                "This user is not assigned to a client organization.";

            return RedirectToPage();
        }

        var selectedUserRoles =
            await _userManager.GetRolesAsync(selectedUser);

        var hasClientPortalAccess =
            selectedUserRoles.Contains(
                RoleNames.ClientAdministrator)
            || selectedUserRoles.Contains(
                RoleNames.ClientUser);

        if (!hasClientPortalAccess)
        {
            StatusMessage =
                "The selected account does not have client portal access.";

            return RedirectToPage();
        }

        var impersonationClaims = new List<Claim>
        {
            new(
                "OriginalAdministratorId",
                administratorId),

            new(
                "IsImpersonating",
                "true"),

            new(
                "ImpersonatedOrganizationId",
                selectedUser.ClientOrganizationId.Value.ToString())
        };

        await _signInManager.SignOutAsync();

        await _signInManager.SignInWithClaimsAsync(
            selectedUser,
            isPersistent: false,
            impersonationClaims);

        return RedirectToPage("/Client/Index");
    }
}