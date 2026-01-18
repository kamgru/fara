using Fara.Web.Features.Admin.Photos;
using Microsoft.AspNetCore.Mvc;

namespace Fara.Web.Controllers;

[ApiController]
public class AdminPhotosController : ControllerBase
{
    [HttpPost]
    [Route("admin/photos/publish/{id}")]
    public async Task<IActionResult> Publish(
        string id,
        [FromServices] IPublishPhotoHandler handler)
    {
        PublishPhotoResult result = await handler.HandleAsync(id);
        return result.IsSuccess
            ? Ok()
            : UnprocessableEntity(result.ErrorMessage);
    }
}
