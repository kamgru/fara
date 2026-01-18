using Fara.Web.Features.Gallery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fara.Web.Pages;

public class IndexModel(IListHandler listHandler) : PageModel
{
    public required List<PhotoListItem> Photos { get; init; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        Photos.AddRange(await listHandler.HandleAsync(1));
        return Page();
    }

    public async Task<IActionResult> OnGetPhotosAsync(int p = 1)
    {
        IReadOnlyList<PhotoListItem> photos = await listHandler.HandleAsync(p);
        return Partial("_PhotosListPartial", photos);
    }
}
