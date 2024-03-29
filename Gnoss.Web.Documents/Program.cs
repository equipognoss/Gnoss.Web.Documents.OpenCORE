using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gnoss.Web.Documents
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
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel(options =>
                    {
                        options.Limits.MaxRequestBufferSize = 1000000000;
                        options.Limits.MaxRequestBodySize = 1000000000;
                    }) ; // Maximo tama�o de subida ~ 1Gb
                    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                });
    }
}
