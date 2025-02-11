using Newtonsoft.Json;

namespace LinodeDDNS;

public class ListDomainRecordsBody
{
    [JsonProperty("page")] public int Page { get; set; }
    [JsonProperty("pages")] public int Pages { get; set; }
    [JsonProperty("results")] public int Results { get; set; }
    [JsonProperty("data")] public ListDomainRecordsBodyItem[] Data { get; set; } = [];
}

public class ListDomainRecordsBodyItem
{
    [JsonProperty("id")] public int Id { get; set; }
    [JsonProperty("name")] public string Name { get; set; } = null!;
    [JsonProperty("type")] public string Type { get; set; } = null!;
    [JsonProperty("target")] public string Target { get; set; } = null!;
}
