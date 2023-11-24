using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System.Text;


namespace EventProcessor.Client;

public class EchoHandler : ChannelHandlerAdapter
{
    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        var msg = message as IByteBuffer;
        if(msg is not null)
        {
            Console.WriteLine($"Received server message:{msg.ToString(Encoding.UTF8)}");
        }
        context.WriteAsync(Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes("Client message!")));
    }
    public override void ChannelReadComplete(IChannelHandlerContext context)
    {
        context.Flush();
    }

    public override void ChannelActive(IChannelHandlerContext context)
    {
        Console.WriteLine( "Sending client message.");
        context.WriteAndFlushManyAsync(Unpooled.CopiedBuffer(Encoding.UTF8.GetBytes("First client message!")));
    }

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        Console.WriteLine(exception);
        context.CloseAsync();
    }
}
