using System.ComponentModel.DataAnnotations;
using ACP.Data;
using ACP.Models.Clients;
using ACP.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ACP.Pages.Admin.Users;

[Authorize(Policy = "SuperAdministrator")]
public class DetailsModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

    public DetailsModel(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public ApplicationUser SelectedUser { get; private set; } = null!;

    public IReadOnlyList<string> Roles { get; private set; } =
        Array.Empty<string>();

    public IReadOnlyList<SelectListItem> Organizations { get; private set; } =
        Array.Empty<SelectListItem>();

    [BindProperty]
    public OrganizationInputModel OrganizationInput { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public sealed class OrganizationInputModel
    {
        [Display(Name = "Existing client company")]
        public int? ClientOrganizationId { get; set; }

        [Display(Name = "New company name")]
        [MaxLength(200)]
        public string? NewCompanyName { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var user = await _userManager.Users
            .AsNoTracking()
            .Include(item => item.ClientOrganization)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (user is null)
        {
            return NotFound();
        }

        SelectedUser = user;

        Roles = (await _userManager.GetRolesAsync(user))
            .OrderBy(role => role)
            .ToArray();

        OrganizationInput = new OrganizationInputModel
        {
            ClientOrganizationId = user.ClientOrganizationId
        };

        await LoadOrganizationsAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostOrganizationAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var user = await _userManager.Users
            .Include(item => item.ClientOrganization)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (user is null)
        {
            return NotFound();
        }

        SelectedUser = user;

        Roles = (await _userManager.GetRolesAsync(user))
            .OrderBy(role => role)
            .ToArray();

        if (!ModelState.IsValid)
        {
            await LoadOrganizationsAsync();
            return Page();
        }

        int? organizationId = OrganizationInput.ClientOrganizationId;

        if (!string.IsNullOrWhiteSpace(
                OrganizationInput.NewCompanyName))
        {
            var companyName =
                OrganizationInput.NewCompanyName.Trim();

            var existingOrganization =
                await _dbContext.ClientOrganizations
                    .FirstOrDefaultAsync(
                        organization =>
                            organization.CompanyName == companyName);

            if (existingOrganization is null)
            {
                existingOrganization = new ClientOrganization
                {
                    CompanyName = companyName,
                    IsActive = true,
                    CreatedUtc = DateTime.UtcNow
                };

                _dbContext.ClientOrganizations.Add(
                    existingOrganization);

                await _dbContext.SaveChangesAsync();
            }

            organizationId = existingOrganization.Id;
        }

        user.ClientOrganizationId = organizationId;

        var updateResult =
            await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            await LoadOrganizationsAsync();
            return Page();
        }

        StatusMessage =
            organizationId.HasValue
                ? "Client company assignment was updated."
                : "Client company assignment was removed.";

        return RedirectToPage(
            "./Details",
            new { id = user.Id });
    }

    private async Task LoadOrganizationsAsync()
    {
        Organizations =
            await _dbContext.ClientOrganizations
                .AsNoTracking()
                .Where(organization => organization.IsActive)
                .OrderBy(organization => organization.CompanyName)
                .Select(organization => new SelectListItem
                {
                    Value = organization.Id.ToString(),
                    Text = organization.CompanyName
                })
                .ToListAsync();
    }
}