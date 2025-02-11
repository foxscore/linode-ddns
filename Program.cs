using LinodeDDNS;
using Newtonsoft.Json;

const string configFilePath = "/etc/linode-ddns.conf";
var config = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(configFilePath));

if (config is not { Version: > 0 })
    throw new Exception("Invalid configuration file");

const int currentConfigFileVersion = 1;
if (config.Version is not currentConfigFileVersion)
    throw new Exception($"Outdated configuration file! Expected {currentConfigFileVersion} but got {config.Version}.");

if (string.IsNullOrWhiteSpace(config.ApiKey))
    throw new Exception("Missing API key");

if (config.Interval < 30)
    throw new Exception(
        "Interval is less than 30 seconds! Set `forceInterval` to true in the config file to continue with this value.");
var delay = config.Interval * 1000;

if (config.Domains.Count == 0)
    throw new Exception("No Domains configured! Nothing to do.");

var lastPublicIp = "";
while (true)
{
    Log.Information("Starting Linode DDNS");
    
    var publicIpResponse = (
        await new SimpleHttp()
            .WithUrl("https://api.ipify.org")
            .GetAsync()
    );
    if (publicIpResponse is not { StatusCode: 200 } || string.IsNullOrWhiteSpace(publicIpResponse.Body))
    {
        Log.Error("Failed to get public IP address!");
        goto AwaitNextLoop;
    }
    var publicIp = publicIpResponse.Body;
    if (lastPublicIp == publicIp)
    {
        Log.Information("Public IP did not change, skipping...");
        goto AwaitNextLoop;
    }

    foreach (var (domainId, subdomainNames) in config.Domains)
    {
        const int maxPageSize = 500;
        var listDomainRecordsResponse = await new SimpleHttp()
            .WithUrl($"https://api.linode.com/v4/domains/{domainId}/records?page=1&page_size={maxPageSize}")
            .WithHeader("Authorization", $"Bearer {config.ApiKey}")
            .WithHeader("Accept", "application/json")
            .GetAsync<ListDomainRecordsBody>();
        if (listDomainRecordsResponse is not { StatusCode: 200, ParsedBody: not null })
        {
            Log.Error($"Failed to get list of domain records for {domainId}");
            continue;
        }
        var listDomainRecords = listDomainRecordsResponse.ParsedBody
            .Data
            .Where(record => record.Type.Equals("A", StringComparison.CurrentCultureIgnoreCase))
            .ToList();

        foreach (var subdomainName in subdomainNames)
        {
            var record = listDomainRecords.FirstOrDefault(r => r.Name.Equals(subdomainName, StringComparison.CurrentCultureIgnoreCase));
            if (record is null)
            {
                Log.Error($"Failed to get record for {subdomainName} of {domainId}");
                continue;
            }
            if (publicIp == record.Target) continue;
            
            var body = new UpdateDomainRecordBody(publicIp);
            var postResult = await new SimpleHttp()
                .WithUrl($"https://api.linode.com/v4/domains/{domainId}/records/{record.Id}")
                .WithHeader("Authorization", $"Bearer {config.ApiKey}")
                .WithHeader("Accept", "application/json")
                .WithBody(body)
                .PostAsync<ListDomainRecordsBodyItem>();
            if (postResult is not { StatusCode: 200, ParsedBody: not null })
                Log.Error($"Failed to update domain record for {domainId} ({postResult?.StatusCode}: {postResult?.Body ?? "<no_message>"})");
            else if (postResult.ParsedBody.Target != publicIp)
                Log.Error($"Failed to update domain record for {domainId} (incorrect echo! expected {publicIp}, got '{postResult.ParsedBody.Target}')");
            else
                Log.Information($"Updated record for domain {subdomainName} of {domainId} to {publicIp}");
        }
    }
    
    Log.Information("Finished Linode DDNS Loop");

    AwaitNextLoop:
    await Task.Delay(delay);
}
