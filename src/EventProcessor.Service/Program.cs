using EventProcessor.Service;
using System.Net;

var exitEvent = new AutoResetEvent(false);
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel(option =>
{
    option.Listen(IPAddress.Loopback, 3614);
});

builder.Services.AddSingleton<ITcpService, TcpService>();

var app = builder.Build();

try
{
    var host = app.Services.GetService<ITcpService>();
    var server = host.CreateServer();
    await server.StartAsync();
    await app.RunAsync();
    exitEvent.WaitOne();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
finally
{

}

