using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sibilus.Web.Server;

namespace Sibilus.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (_, _) => ExitApplication();
            AppDomain.CurrentDomain.ProcessExit += (_, _) => ExitApplication();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

#if DEBUG
                    webBuilder.UseUrls("http://*:8080");
#else
                    webBuilder.UseUrls("http://*:80", "https://*:443");
#endif
                });

        private static void ExitApplication()
        {
            DbCache.DbClient?.Dispose();
        }
    }
}
