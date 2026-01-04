using Fara.Web.Pages.Admin;
using Microsoft.AspNetCore.Mvc;

namespace Fara.Web.Controllers;

//TODO: need another controller that serves only published photos,
//this one should serve all for the admin backend
//should the photos be moved to another location after publishing to avoid having to call db to check status?

[ApiController]
public class AdminImagesController(
    IWebHostEnvironment env,
    ILogger<AdminImagesController> logger) : ControllerBase
{
    [HttpGet]
    [Route("images/{id}")]
    public async Task<IActionResult> GetAsync(string id)
    {
        if (!FilenameValidator.IsValidFilename(id))
        {
            return NotFound();
        }

        if (!id.Contains('_'))
        {
            id = $"{id}_n";
        }

        string root = Path.GetFullPath(Path.Combine(env.ContentRootPath, "photos"));

        try
        {
            await Task.CompletedTask;
            Stream s = new FileStream($"{root}/{id}.jpg", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return File(s, "image/jpeg");
        }
        catch(Exception e)
        {
            logger.LogWarning(e, "Could not load image {id}", id);
            return NotFound();
        }
    }
}
