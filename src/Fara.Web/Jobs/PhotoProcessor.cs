using System.Threading.Channels;
using Fara.Web.Features.Admin.Photos;

namespace Fara.Web.Jobs;

public class PhotoProcessingQueue
{
    private readonly Channel<string> chan = Channel.CreateUnbounded<string>();

    public async Task EnqueueAsync(string key) => await chan.Writer.WriteAsync(key);
    public async Task<string> DequeueAsync() => await chan.Reader.ReadAsync();
}

public class PhotoProcessor(
    PhotoProcessingQueue queue,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            string photoId = await queue.DequeueAsync();
            using IServiceScope scope = scopeFactory.CreateScope();
            IProcessHandler handler = scope.ServiceProvider.GetRequiredService<IProcessHandler>();
            await handler.HandleAsync(photoId);
        }
    }
}
