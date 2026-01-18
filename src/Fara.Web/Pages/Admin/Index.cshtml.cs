using Fara.Web.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Fara.Web.Pages.Admin;

public class Index : PageModel
{
    public List<PhotoEntry> Photos { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int p = 0)
    {
        if (p is < 0 or > 1000)
        {
            p = 0;
        }

        int offset = p * 25;

        await using SqliteConnection con = new("Data Source=photos.db");
        await con.OpenAsync();
        await using SqliteCommand cmd = con.CreateCommand();
        cmd.CommandText = """
                            select photoId, state from photos
                            limit 25 offset @offset;
                          """;
        cmd.Parameters.AddWithValue("@offset", offset);

        await using SqliteDataReader reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            string key = reader.GetString(0);
            int status = reader.GetInt32(1);
            PhotoEntry entry = new($"admin/images/{key}_t", (PublicationState)status, key);
            Photos.Add(entry);
        }

        return Page();
    }
}

public record PhotoEntry(
    string Url,
    PublicationState Status,
    string PhotoId);

