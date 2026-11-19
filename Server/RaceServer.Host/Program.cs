using ApexGTWorld.RaceServer.Host;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "APEX GT WORLD Race Server";
});
builder.Services.AddHostedService<RaceServerWorker>();

await builder.Build().RunAsync();
