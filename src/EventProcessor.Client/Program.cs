// See https://aka.ms/new-console-template for more information
using DotNetty.Buffers;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using EventProcessor.Client;
using System.Net;

await runAsync();

async Task runAsync()
{
    var group = new MultithreadEventLoopGroup(Environment.ProcessorCount);
	try
	{
        var bootstrap = new Bootstrap()
        .Group(group)
        .Channel<TcpSocketChannel>()
        .Option(ChannelOption.Allocator, UnpooledByteBufferAllocator.Default)
        .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
        {
            IChannelPipeline pipeline = channel.Pipeline;
            pipeline.AddLast(new LoggingHandler(LogLevel.DEBUG));
            pipeline.AddLast(new EchoHandler());
        }));

        IChannel client = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3600)); //Server address
        Console.ReadLine();// keep running
        await client.CloseAsync();
    }
    catch (Exception ex)
    {

        Console.WriteLine(ex);
    }
    finally
    {
        await group.ShutdownGracefullyAsync();
 
    }
}