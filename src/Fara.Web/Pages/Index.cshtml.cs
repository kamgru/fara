using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Fara.Web.Pages;

public class IndexModel : PageModel
{
    public required List<PhotoEntry> Photos { get; init; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        await using SqliteConnection con = new SqliteConnection("data source=photos.db");
        await con.OpenAsync();
        await using SqliteCommand cmd = con.CreateCommand();
        cmd.CommandText = """
                            select key from photos
                          """;
        await using SqliteDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            string key = reader.GetString(0);
            Photos.Add(new PhotoEntry($"images/{key}_n"));
        }

        return Page();
    }
}

public record PhotoEntry(string Url);
