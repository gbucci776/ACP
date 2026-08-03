using System.ComponentModel.DataAnnotations;
using ACP.Data.Identity;
using ACP.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ACP.Pages.Consumer;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(
        string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/consumer");

        if (!ModelState.IsValid)
        {
            ReturnUrl = returnUrl;
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);

        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(
                string.Empty,
                "Unable to sign in with those credentials.");

            ReturnUrl = returnUrl;
            return Page();
        }

        var isConsumer = await _userManager.IsInRoleAsync(
            user,
            RoleNames.Consumer);

        if (!isConsumer)
        {
            ModelState.AddModelError(
                string.Empty,
                "Unable to sign in with those credentials.");

            ReturnUrl = returnUrl;
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            user.LastLoginUtc = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "This account is temporarily locked.");
        }
        else
        {
            ModelState.AddModelError(
                string.Empty,
                "Unable to sign in with those credentials.");
        }

        ReturnUrl = returnUrl;
        return Page();
    }
}