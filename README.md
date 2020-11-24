# Cloudflare DDNS Update Client

Dynamic DNS using Cloudflare.

.NET Core client that updates Cloudflare DNS with public IP address when it changes.

## Configuration

Set the following values in the appsettings.json file:

**ZoneId**

Log in to Cloudflare, go to domain overview, the Zone ID is on the right hand side in the "API" section.

**RecordId**

Log in to Cloudflare, go to domain DNS, open inspector to look at network traffic, update the DNS record, copy the record ID from the end of the URL.

**RecordName**

The name of the record you want to update. For example, "subdomain.domain.com" (for a CNAME) or "domain.com" (for the zone apex).

**RecordType**

The type of the record being updated. For example, "A" or "CNAME".

**ApiToken**

Log in to Cloudflare, go to profile, go to API Tokens. Create a toekn with these permissions: Zone.Zone Settings, Zone.Zone, Zone.DNS
