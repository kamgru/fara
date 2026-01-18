using System.ComponentModel.DataAnnotations;
using Fara.Web.Features.Admin.Photos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Fara.Web.Pages.Admin;

public class Upload(
    IUploadHandler uploadHandler,
    ILogger<Upload> logger) : PageModel
{
    [BindProperty] [Required] public List<IFormFile> Input { get; set; } = [];

    public string? Message { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Message = "No file selected";
            return Page();
        }

        foreach (IFormFile inputFile in Input)
        {
            try
            {
                await uploadHandler.HandleAsync(inputFile);
            }
            catch (Exception ex)
            {
                Message = "There was an error saving the file";
                logger.LogError(ex, "There was an error saving the file");
            }
        }

        Message = "File saved";
        return Page();
    }
}
