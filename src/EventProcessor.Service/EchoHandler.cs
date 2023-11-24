using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System.Text;

namespace EventProcessor.Service;

public interface IEchoMessageHandler : IChannelHandler
{

}
public class EchoHandler : ChannelHandlerAdapter, IEchoMessageHandler
{
    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        var msg = message as IByteBuffer;
        if (msg is not null)
        {
            Console.WriteLine($"Received client message:{msg.ToString(Encoding.UTF8)}");
        }
        context.WriteAsync(Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes("Server message!")));
    }
    public override void ChannelReadComplete(IChannelHandlerContext context)
    {
        context.Flush();
    }

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        Console.WriteLine(exception);
        context.CloseAsync();
    }
}
