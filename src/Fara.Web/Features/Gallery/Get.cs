
using System.Data;
using Fara.Web.Common;
using Fara.Web.DataAccess;

namespace Fara.Web.Features.Gallery;
public record GetPhotoResult(Stream Stream, string ContentType);

public interface IGetPhotoHandler
{
    Task<GetPhotoResult?> HandleAsync(
        string filename);
}

public class GetPhotoHandler(
    ISqlQueryRunner queryRunner,
    IPhotoStorage storage) : IGetPhotoHandler, IScoped
{
    public async Task<GetPhotoResult?> HandleAsync(
        string filename)
    {
        if (!FilenameValidator.IsValidFilename(filename))
        {
            return null;
        }

        string? photoId = filename.Split('_').FirstOrDefault();
        if (string.IsNullOrEmpty(photoId))
        {
            return null;
        }

        PublicationState? publicationState = await queryRunner.GetAsync<PublicationState?>(
            "select state from photos where photoId = @photoId;",
            reader =>
            {
                int state = reader.GetInt32(0);
                if (!Enum.IsDefined(typeof(PublicationState), state))
                {
                    return null;
                }

                return (PublicationState)state;
            },
            new SqlArgument("@photoId", photoId, DbType.String));

        if (publicationState is not PublicationState.Published)
        {
            return null;
        }

        Stream stream = storage.OpenRead($"{filename}", "published");
        return new GetPhotoResult(stream, "image/jpeg");
    }
}
