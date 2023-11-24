using EventProcessor.Service;
using System.Net;

try
{
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>()
            .UseKestrel(opt => { opt.Listen(IPAddress.Loopback, 3500); }); // http listening for health check etc.
        })
        .Build()
        .Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
