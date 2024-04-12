namespace CloudflareDDNSUpdater;

public class RecordConfiguration
{
    public string ZoneId { get; set; }

    public string RecordId { get; set; }

    public string RecordName { get; set; }

    public string RecordType { get; set; }

    public int RecordTtl { get; set; }
}