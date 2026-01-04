using System.Threading.Channels;
using Microsoft.Data.Sqlite;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Scan(scan =>
    scan.FromApplicationDependencies()
        .AddClasses(classes => classes.AssignableTo<IScoped>())
        .AsImplementedInterfaces()
        .WithScopedLifetime());

builder.Services.AddSingleton<PhotoProcessingQueue>();
builder.Services.AddHostedService<PhotoProcessor>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
    {
        o.MultipartBodyLengthLimit = 120 * 2048 * 2048;
    });

    builder.WebHost.ConfigureKestrel(o =>
    {
        o.Limits.MaxRequestBodySize = 120 * 2048 * 2048;
    });
}

var app = builder.Build();

{
    using var con = new SqliteConnection("Data Source=photos.db");
    con.Open();
    SqliteCommand cmd = con.CreateCommand();
    cmd.CommandText = """
                      create table if not exists photos (
                          id integer not null  primary key autoincrement,
                          key text not null,
                          status text not null default('processing'),
                          created_at text not null default(CURRENT_TIMESTAMP)
                          );
                      """;
    cmd.ExecuteNonQuery();
}

// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();
app.MapControllers();

app.Run();

internal interface IScoped;

public class PhotoProcessingQueue
{
    private readonly Channel<string> chan =  Channel.CreateUnbounded<string>();

    public async Task EnqueueAsync(string key) => await chan.Writer.WriteAsync(key);
    public async Task<string> DequeueAsync() => await chan.Reader.ReadAsync();
}

public class PhotoProcessor(
    PhotoProcessingQueue queue,
    IWebHostEnvironment env) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            string key = await queue.DequeueAsync();
            string root = Path.GetFullPath(Path.Combine(env.ContentRootPath, "photos", "source"));
            string filename = Path.GetFullPath(Path.Combine(root, $"{key}.jpg"));

            Image img = await Image.LoadAsync(filename, stoppingToken);

            img.Mutate(x => x.Resize(
                new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(1024, 1024)
                }));
            string target = Path.GetFullPath(Path.Combine(env.ContentRootPath, "photos"));
            await img.SaveAsJpegAsync(Path.Combine(target, $"{key}_n.jpg"), stoppingToken);

            img.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(300, 300)
            }));
            string thumb = Path.GetFullPath(Path.Combine(env.ContentRootPath, "photos"));
            await img.SaveAsJpegAsync(Path.Combine(thumb, $"{key}_t.jpg"), stoppingToken);

            await using SqliteConnection con = new("Data Source=photos.db");
            await con.OpenAsync(stoppingToken);
            await using SqliteCommand cmd = con.CreateCommand();
            cmd.CommandText = """
                                update photos
                                set status = 'ready'
                                where key = @key;
                              """;
            cmd.Parameters.AddWithValue("@key", key);
            await cmd.ExecuteNonQueryAsync(stoppingToken);
        }
    }
}
