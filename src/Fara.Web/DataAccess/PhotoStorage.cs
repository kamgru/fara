using System.Text.RegularExpressions;

namespace Fara.Web.DataAccess;

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

public interface IPhotoStorageRootPathProvider
{
    string Root { get; }
}

public class PhotoStorageRootPathProvider(
    IWebHostEnvironment env) : IPhotoStorageRootPathProvider
{
    public string Root =>
        Path.GetFullPath(Path.Combine(env.ContentRootPath, "photos"));
}

public interface IPhotoStorage
{
    Task SaveAsync(string photoId, Stream inputStream, string container);
    Stream OpenRead(string photoId, string container);
    void Move(string photoId, string sourceContainer, string targetContainer);
}

public class PhotoStorage(IWebHostEnvironment env) : IPhotoStorage, IScoped
{
    private const string Root = "photos";

    public async Task SaveAsync(string photoId, Stream inputStream, string container = "")
    {
        string root = Path.GetFullPath(Path.Combine(env.ContentRootPath, Root, container));
        Directory.CreateDirectory(root);

        string tmp = Path.Combine(root, Path.GetTempFileName());
        await using FileStream fs = File.Create(tmp);
        await inputStream.CopyToAsync(fs);

        string actual = Path.Combine(root, $"{photoId}.jpg");
        File.Move(tmp, actual, true);
    }

    public Stream OpenRead(string photoId, string container)
    {
        string filename = Path.GetFullPath(Path.Combine(env.ContentRootPath, Root, container, $"{photoId}.jpg"));
        return File.OpenRead(filename);
    }

    public void Move(string photoId, string sourceContainer, string targetContainer)
    {
        string source = Path.GetFullPath(Path.Combine(env.ContentRootPath, Root, sourceContainer, $"{photoId}.jpg"));
        string targetDirectory = Path.GetFullPath(Path.Combine(env.ContentRootPath, Root, targetContainer));
        Directory.CreateDirectory(targetDirectory);

        string targetFilename = Path.Combine(targetDirectory, $"{photoId}.jpg");

        File.Move(source, targetFilename, true);
    }
}
