using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CloudflareDDNSUpdater;

public class DnsUpdater {
    private readonly ILogger<DnsUpdater> _logger;

    private readonly IConfiguration _configuration;

    private static string _lastIpAddress;

    public DnsUpdater(
        ILogger<DnsUpdater> logger,
        IConfiguration configuration) {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task Update() {
        var ipAddress = await GetPublicIpAddress();

        if (string.IsNullOrEmpty(ipAddress)) {
            return;
        }

        _logger.LogTrace("IP address is {IpAddress}", ipAddress);

        if (_lastIpAddress != null && _lastIpAddress.Equals(ipAddress, StringComparison.OrdinalIgnoreCase)) {
            _logger.LogTrace("IP has not changed, update not required");
            return;
        }

        await UpdateDns(ipAddress);

        _lastIpAddress = ipAddress;
            
        _logger.LogInformation("DNS updated to use IP: {IpAddress}", ipAddress);
    }

    private async Task<string> GetPublicIpAddress() {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(15);

        try {
            var response = await client.GetAsync("https://api.ipify.org/");

            response.EnsureSuccessStatusCode();

            return (await response.Content.ReadAsStringAsync()).Trim();
        }
        catch (OperationCanceledException) {
            _logger.LogError("Operation canceled or timeout getting public IP address");
            return null;
        }
    }

    private async Task UpdateDns(string ipAddress)
    {
        var apiToken = _configuration.GetValue<string>("Cloudflare:ApiToken");
        var recordConfigs = _configuration.GetSection("Cloudflare:Records").GetChildren();

        foreach (var recordConfig in recordConfigs)
        {
            var config = recordConfig.Get<RecordConfiguration>();

            try
            {
                await UpdateDnsForZone(ipAddress, config, apiToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating zone {ZoneId}, record {RecordId}", config.ZoneId, config.RecordId);
            }
        }
    }

    private static async Task UpdateDnsForZone(string ipAddress, RecordConfiguration config, string apiToken)
    {
        var url = $"https://api.cloudflare.com/client/v4/zones/{config.ZoneId}/dns_records/{config.RecordId}";

        using var client = new HttpClient();

        var json = JsonSerializer.Serialize(new {
            content = ipAddress,
            name = config.RecordName,
            ttl = config.RecordTtl,
            type = config.RecordType
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var message = new HttpRequestMessage {
            Method = HttpMethod.Put,
            RequestUri = new Uri(url),
            Content = content,
            Headers = {
                { "Authorization", $"Bearer {apiToken}"  }
            }
        };

        var response = await client.SendAsync(message);
        
        response.EnsureSuccessStatusCode();
    }
}