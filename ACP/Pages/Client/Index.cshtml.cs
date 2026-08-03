using ACP.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACP.Pages.Client;

[Authorize(Policy = "ClientPortal")]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public string CompanyName { get; private set; } = "Client Account";

    public string UserDisplayName { get; private set; } = string.Empty;

    public string CompanyInitials { get; private set; } = "CA";

    public async Task OnGetAsync()
    {
        var currentUserId =
            _userManager.GetUserId(User);

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return;
        }

        var user = await _userManager.Users
            .AsNoTracking()
            .Include(item => item.ClientOrganization)
            .FirstOrDefaultAsync(item => item.Id == currentUserId);

        if (user is null)
        {
            return;
        }

        UserDisplayName =
            $"{user.FirstName} {user.LastName}".Trim();

        CompanyName =
            user.ClientOrganization?.CompanyName
            ?? "Client Account";

        CompanyInitials = GetInitials(CompanyName);
    }

    private static string GetInitials(string companyName)
    {
        var words = companyName
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries
                | StringSplitOptions.TrimEntries);

        if (words.Length == 0)
        {
            return "CA";
        }

        return string.Concat(
                words.Take(2)
                    .Select(word => char.ToUpperInvariant(word[0])))
            .PadRight(2, 'A');
    }
}