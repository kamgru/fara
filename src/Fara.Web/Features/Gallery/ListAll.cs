using System.Data;
using Fara.Web.DataAccess;

namespace Fara.Web.Features.Gallery;

public record PhotoListItem(string Url);

public interface IListHandler
{
    Task<IReadOnlyList<PhotoListItem>> HandleAsync(int page);
}

public class ListHandler(
    ISqlQueryRunner queryRunner) : IListHandler, IScoped
{
    private const int PageSize = 5;

    public async Task<IReadOnlyList<PhotoListItem>> HandleAsync(int page)
    {
        if (page < 1)
        {
            page = 1;
        }

        int offset = (page - 1) * PageSize;

        IReadOnlyList<PhotoListItem> result = await queryRunner.QueryAsync<PhotoListItem>(
            "select photoId from photos where state = 2 limit @pageSize offset @offset",
            reader =>
            {
                string key = reader.GetString(0);
                return new PhotoListItem($"images/{key}_n");
            },
            new SqlArgument("@pageSize", PageSize, DbType.Int32),
            new SqlArgument("@offset", offset, DbType.Int32));

        return result;
    }
}
