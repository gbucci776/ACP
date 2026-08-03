using ACP.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ACP.Pages.Admin.Users;

[Authorize(Policy = "SuperAdministrator")]
public class DetailsModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DetailsModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public ApplicationUser SelectedUser { get; private set; } = null!;

    public IReadOnlyList<string> Roles { get; private set; } =
        Array.Empty<string>();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        SelectedUser = user;

        Roles = (await _userManager.GetRolesAsync(user))
            .OrderBy(role => role)
            .ToArray();

        return Page();
    }
}