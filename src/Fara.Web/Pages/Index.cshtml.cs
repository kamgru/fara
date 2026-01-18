using Fara.Web.Features.Gallery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fara.Web.Pages;

public class IndexModel(IListAllHandler listAllHandler) : PageModel
{
    public required List<PhotoListItem> Photos { get; init; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        Photos.AddRange(await listAllHandler.HandleAsync());
        return Page();
    }
}
