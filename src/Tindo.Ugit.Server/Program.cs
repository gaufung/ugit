using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Tindo.Ugit.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                              .UseKestrel((context, options) =>
                              {
                                  if (context.HostingEnvironment.IsProduction())
                                  {
                                      options.Listen(System.Net.IPAddress.Any, 44321, options => { options.UseHttps(); });
                                  }
                                  
                              });
                });
    }
}
