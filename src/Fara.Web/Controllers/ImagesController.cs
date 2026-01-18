using Fara.Web.Features.Gallery;
using Microsoft.AspNetCore.Mvc;

namespace Fara.Web.Controllers;

[ApiController]
public class ImagesController : ControllerBase
{
    [HttpGet]
    [Route("images/{filename}")]
    public async Task<IActionResult> GetAsync(
        string filename,
        [FromServices] IGetPhotoHandler handler)
    {
        GetPhotoResult?  result = await handler.HandleAsync(filename);
        return result != null
            ? File(result.Stream, result.ContentType)
            : NotFound();
    }

    [HttpGet]
    [Route("images")]
    public async Task<IActionResult> ListAsync(
        int page,
        [FromServices] IListHandler handler)
    {
        IReadOnlyList<PhotoListItem> result = await handler.HandleAsync(page);
        return Ok(result);
    }
}
