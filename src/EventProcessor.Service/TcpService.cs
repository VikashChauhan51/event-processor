using DotNetty.Buffers;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System.Net;

namespace EventProcessor.Service;

public interface ITcpService
{
    bool IsActive { get; }
    ITcpService CreateServer();
    Task<IChannel> StartAsync();
    Task StopAsync();
}

public class TcpService : ITcpService
{
    private ServerBootstrap? bootstrap;
    private MultithreadEventLoopGroup bossEventGroup;
    private MultithreadEventLoopGroup workerEventGroup;
    private IChannel? channel;

    public bool IsActive => channel?.IsActive ?? false;
    public ITcpService CreateServer()
    {
        bossEventGroup = new MultithreadEventLoopGroup(10);
        workerEventGroup = new MultithreadEventLoopGroup(5);
        bootstrap = new ServerBootstrap();
        bootstrap
            .Group(bossEventGroup, workerEventGroup)
            .Channel<TcpServerSocketChannel>()
            .ChildOption(ChannelOption.SoKeepalive, true)
            .Option(ChannelOption.Allocator, UnpooledByteBufferAllocator.Default)
            .Option(ChannelOption.SoBacklog, 100)
            .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
            {
                channel.Pipeline.AddLast(new LoggingHandler(DotNetty.Handlers.Logging.LogLevel.DEBUG));
                channel.Pipeline.AddLast(new EchoHandler());
            }));


        return this;
    }

    public async Task<IChannel> StartAsync()
    {
        if (!IsActive)
        {
            channel = await bootstrap!.BindAsync(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 3600));
        }

        return channel!;
    }

    public async Task StopAsync()
    {
        if (IsActive)
        {
            await channel!.CloseAsync();
            await Task.WhenAll(bossEventGroup.ShutdownGracefullyAsync(), workerEventGroup.ShutdownGracefullyAsync());

        }
        bootstrap = null;
        channel = null;
    }
}
