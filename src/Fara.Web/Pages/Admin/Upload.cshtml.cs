using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Fara.Web.Pages.Admin;

public class Upload(
    IPhotoStorage photoStorage,
    PhotoProcessingQueue queue,
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
                logger.LogInformation($"Uploading {inputFile.FileName}");
                string key = Guid.NewGuid().ToString("N");
                await photoStorage.SaveAsync(key, inputFile.OpenReadStream(), "source");

                await using SqliteConnection con = new("Data Source=photos.db");
                await con.OpenAsync();
                await using SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText = "insert into photos (key) values (@key)";
                cmd.Parameters.AddWithValue("@key", key);
                await cmd.ExecuteNonQueryAsync();

                await queue.EnqueueAsync(key);
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

public static partial class FilenameValidator
{
    [GeneratedRegex(
        "^[a-zA-Z0-9]{32}(_[a-z])?$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
    )]
    private static partial Regex SafeJpegFileNameRegex();

    public static bool IsValidFilename(string filename)
    {
        return SafeJpegFileNameRegex().IsMatch(filename);
    }
}

public interface IPhotoStorage
{
    Task SaveAsync(string key, Stream inputStream, string target);
}

public class PhotoStorage(IWebHostEnvironment env) : IPhotoStorage, IScoped
{
    public async Task SaveAsync(string key, Stream inputStream, string target = "")
    {
        if (!FilenameValidator.IsValidFilename(key))
        {
            throw new InvalidOperationException("Key contains invalid characters");
        }

        //TODO: check first bytes to see if really jpg

        string root = Path.GetFullPath(Path.Combine(env.ContentRootPath, "photos", target));
        Directory.CreateDirectory(root);

        string tmp = Path.Combine(root, Path.GetTempFileName());
        await using FileStream fs = File.Create(tmp);
        await inputStream.CopyToAsync(fs);

        string actual = Path.Combine(root, $"{key}.jpg");
        File.Move(tmp, actual);
    }
}
