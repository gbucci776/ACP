using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ACP.Pages.Consumer;

[Authorize(Policy = "ConsumerPortal")]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}