using Newtonsoft.Json;

namespace LinodeDDNS;

public class ConfigFile
{
    [JsonProperty("version", Required = Required.Always)] public int Version { get; set; }
    [JsonProperty("apiKey", Required = Required.Always)] public string ApiKey { get; set; } = null!;
    [JsonProperty("interval", Required = Required.Default)] public int Interval { get; set; } = 30;
    [JsonProperty("forceInterval", Required = Required.Default)] public bool ForceInterval { get; set; } = false;
    [JsonProperty("domains", Required = Required.Default)] public Dictionary<string, string[]> Domains { get; set; } = new();
}
