using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CloudflareDDNSUpdater {
    public class DnsUpdater {
        private readonly ILogger<DnsUpdater> logger;

        private readonly IConfiguration configuration;

        private static string lastIpAddress;

        public DnsUpdater(
            ILogger<DnsUpdater> logger,
            IConfiguration configuration) {
            this.logger = logger;
            this.configuration = configuration;
        }

        public async Task Update() {
            var ipAddress = await GetPublicIpAddress();

            if (string.IsNullOrEmpty(ipAddress)) {
                return;
            }

            logger.LogTrace("IP address is {0}", ipAddress);

            if (lastIpAddress != null && lastIpAddress.Equals(ipAddress, StringComparison.OrdinalIgnoreCase)) {
                logger.LogTrace("IP has not changed, update not required.");
                return;
            }

            await UpdateDns(ipAddress);

            lastIpAddress = ipAddress;
            
            logger.LogInformation($"DNS updated to use IP: {ipAddress}");
        }

        private async Task<string> GetPublicIpAddress() {
            using var client = new HttpClient {
                Timeout = TimeSpan.FromSeconds(15)
            };

            try {
                var response = await client.GetAsync("https://api.ipify.org/");

                response.EnsureSuccessStatusCode();

                return (await response.Content.ReadAsStringAsync())?.Trim();
            }
            catch (TaskCanceledException) {
                logger.LogError("Timeout getting public IP address.");
                return null;
            }
        }

        private async Task UpdateDns(string ipAddress) {
            var config = configuration.GetSection("Cloudflare");

            var zoneId = config.GetValue<string>("ZoneId");
            var recordId = config.GetValue<string>("RecordId");

            var url = $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records/{recordId}";

            using var client = new HttpClient();

            var json = JsonSerializer.Serialize(new {
                content = ipAddress,
                name = config.GetValue<string>("RecordName"),
                ttl = config.GetValue<int>("RecordTTL"),
                type = config.GetValue<string>("RecordType")
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var token = config.GetValue<string>("ApiToken");

            var message = new HttpRequestMessage {
                Method = HttpMethod.Put,
                RequestUri = new Uri(url),
                Content = content,
                Headers = {
                    { "Authorization", $"Bearer {token}"  }
                }
            };

            var response = await client.SendAsync(message);

            response.EnsureSuccessStatusCode();
        }
    }
}
