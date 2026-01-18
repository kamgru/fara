using Fara.Web.DataAccess;

namespace Fara.Web.Features.Gallery;

public record PhotoListItem(string Url);

public interface IListAllHandler
{
    Task<IReadOnlyList<PhotoListItem>> HandleAsync();
}

public class ListAllHandler(
    ISqlQueryRunner queryRunner) : IListAllHandler, IScoped
{
    public async Task<IReadOnlyList<PhotoListItem>> HandleAsync()
    {
        IReadOnlyList<PhotoListItem> result = await queryRunner.QueryAsync<PhotoListItem>(
            "select photoId from photos where state = 2 limit 3",
            reader =>
            {
                string key = reader.GetString(0);
                return new PhotoListItem($"images/{key}_n");
            });

        return result;
    }
}
