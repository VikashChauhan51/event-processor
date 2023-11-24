using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Channels;
using System.Net;
using DotNetty.Handlers.Logging;
using EventProcessor.Server;

await runAsync();

async Task runAsync()
{
    var bossEventGroup = new MultithreadEventLoopGroup(1);
    var workerEventGroup = new MultithreadEventLoopGroup();

    try
    {

        var bootstrap = new ServerBootstrap();
        bootstrap
            .Group(bossEventGroup, workerEventGroup)
            .Channel<TcpServerSocketChannel>()
            .ChildOption(ChannelOption.SoKeepalive, true)
            .Option(ChannelOption.Allocator, UnpooledByteBufferAllocator.Default)
            .Option(ChannelOption.SoBacklog, 100)
            .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
            {
                IChannelPipeline pipeline = channel.Pipeline;
                pipeline.AddLast(new LoggingHandler(LogLevel.DEBUG));
                pipeline.AddLast(new EchoHandler());
            }));

        IChannel client = await bootstrap.BindAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3600));
        Console.ReadLine();// keep running
        await client.CloseAsync();
    }
    catch (Exception ex)
    {

        Console.WriteLine(ex);
    }
    finally
    {
        await bossEventGroup.ShutdownGracefullyAsync();
        await workerEventGroup.ShutdownGracefullyAsync();
    }
}