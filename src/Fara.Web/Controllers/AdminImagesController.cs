using Fara.Web.Features.Admin.Photos;
using Microsoft.AspNetCore.Mvc;

namespace Fara.Web.Controllers;

[ApiController]
public class AdminImagesController : ControllerBase
{
    [HttpGet]
    [Route("admin/images/{id}")]
    public async Task<IActionResult> IndexAsync(
        string id,
        [FromServices] IGetAdminPhotoHandler handler)
    {
        GetPhotoResult? result = await handler.HandleAsync(id);
        return result != null
            ? File(result.Stream, result.ContentType)
            : NotFound();
    }
}
