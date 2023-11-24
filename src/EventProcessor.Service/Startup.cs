using DotNetty.Buffers;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace EventProcessor.Service;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddHostedService<NettyServerHostedService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();

        });
    }
}

public class NettyServerHostedService : IHostedService
{
    private IEventLoopGroup bossGroup;
    private IEventLoopGroup workerGroup;
    private IChannel bootstrapChannel;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        bossGroup = new MultithreadEventLoopGroup(1);
        workerGroup = new MultithreadEventLoopGroup();

        try
        {
            var bootstrap = new ServerBootstrap();
            bootstrap
                .Group(bossGroup, workerGroup)
                .Channel<TcpServerSocketChannel>()
                .ChildOption(ChannelOption.SoKeepalive, true)
                .Option(ChannelOption.Allocator, UnpooledByteBufferAllocator.Default)
                .Option(ChannelOption.SoBacklog, 100)
                .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    channel.Pipeline.AddLast(new LoggingHandler(DotNetty.Handlers.Logging.LogLevel.DEBUG));
                    channel.Pipeline.AddLast(new EchoHandler());
                }));

            bootstrapChannel = await bootstrap.BindAsync(3600); // Bind to the port you want.
        }
        catch
        {
            await StopAsync(cancellationToken);
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await bootstrapChannel.CloseAsync();
        }
        finally
        {
            await Task.WhenAll(
                bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
        }
    }
}

