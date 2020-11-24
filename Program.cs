using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CloudflareDDNSUpdater {
    class Program {
        private static async Task Main() {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => {
                    builder.AddConsole(options => {
                        options.TimestampFormat = "dd MMM hh:mm:ss ";
                    });

                    builder.AddConfiguration(configuration);
                })
                .AddTransient<DnsUpdater>()
                .AddTransient(p => configuration)
                .BuildServiceProvider();

            while (true) {
                var dnsUpdater = serviceProvider.GetService<DnsUpdater>();
                var logger = serviceProvider.GetService<ILogger<Program>>();

                try {
                    await dnsUpdater.Update();
                }
                catch (Exception ex) {
                    logger.LogError(ex, "Error during DNS update.");
                }

                logger.LogTrace("Waiting for next update.");

                await Task.Delay(TimeSpan.FromMinutes(15));
            }
        }
    }
}
