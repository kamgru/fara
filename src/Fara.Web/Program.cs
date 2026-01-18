using Fara.Web.DataAccess;
using Fara.Web.Features.Admin.Photos;
using Fara.Web.Jobs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.Scan(scan =>
    scan.FromApplicationDependencies()
        .AddClasses(classes => classes.AssignableTo<IScoped>())
        .AsImplementedInterfaces()
        .WithScopedLifetime());

PhotoProcessingQueue queue = new();
builder.Services.AddSingleton(queue);
builder.Services.AddHostedService<PhotoProcessor>();
builder.Services.AddScoped<ISqlCommandExecutor>(_ => new SqlCommandExecutor("data source=photos.db"));
builder.Services.AddScoped<ISqlQueryRunner>(_ => new SqlQueryRunner("data source=photos.db"));

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

WebApplication app = builder.Build();
{
    string sql = """
                      create table if not exists photos (
                          id integer not null  primary key autoincrement,
                          photoId text not null,
                          state integer not null default(0),
                          created_at text not null default(CURRENT_TIMESTAMP)
                          );
                      """;
    ISqlCommandExecutor command = new SqlCommandExecutor("data source=photos.db");
    await command.ExecuteAsync(sql);

    ISqlQueryRunner sqlQueryRunner = new SqlQueryRunner("data source=photos.db");
    IReadOnlyList<string> photoIds = await sqlQueryRunner.QueryAsync<string>(
        "select photoId from photos where state = 0;",
        reader => reader.GetString(0));
    foreach (string photoId in photoIds)
    {
        await queue.EnqueueAsync(photoId);
    }
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

