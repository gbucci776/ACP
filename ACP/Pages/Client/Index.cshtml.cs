using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ACP.Pages.Client;

[Authorize(Policy = "ClientPortal")]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}