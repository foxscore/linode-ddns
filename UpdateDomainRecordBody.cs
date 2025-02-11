using Newtonsoft.Json;

namespace LinodeDDNS;

public class UpdateDomainRecordBody(string target)
{
    [JsonProperty("target")] public readonly string Target = target;
}