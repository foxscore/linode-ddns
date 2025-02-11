using Newtonsoft.Json;

namespace LinodeDDNS;

public class SimpleHttpResponse(int statusCode, string body)
{
    public readonly int StatusCode = statusCode;
    public readonly string Body = body;
}

public class SimpleHttpResponse<T> : SimpleHttpResponse
{
    public readonly T? ParsedBody;

    public SimpleHttpResponse(int statusCode, string body) : base(statusCode, body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            ParsedBody = default;
            return;
        }
        
        try
        {
            ParsedBody = JsonConvert.DeserializeObject<T>(Body);
        }
        catch (Exception e)
        {
            Log.Error("Failed to deserialize HTTP response");
            Log.Exception(e);
            ParsedBody = default;
        }
    }
}

public class SimpleHttp
{
    private readonly HttpClient _httpClient = new();
    private string? _url;
    private string? _body;

    public SimpleHttp()
    {
        _httpClient.DefaultRequestHeaders.Remove("User-Agent");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "LinodeDDNS/1.0 (https://github.com/foxscore/linode-ddns)");
    }

    public SimpleHttp WithUrl(string url)
    {
        _url = url;
        return this;
    }

    public SimpleHttp WithHeader(string key, string value)
    {
        _httpClient.DefaultRequestHeaders.Add(key, value);
        return this;
    }

    public SimpleHttp WithBody(string value)
    {
        _body = value;
        return this;
    }

    public SimpleHttp WithBody<T>(T value)
    {
        _body = JsonConvert.SerializeObject(value);
        return this;
    }

    public async Task<SimpleHttpResponse?> PostAsync()
    {
        if (_url is null) throw new NullReferenceException("Url has not been set");
        try
        {
            var result = await _httpClient.PostAsync(_url, _body is null ? null : new StringContent(_body));
            var content = await result.Content.ReadAsStringAsync();
            return new SimpleHttpResponse((int)result.StatusCode, content);
        }
        catch (Exception ex)
        {
            Log.Exception(ex);
            return null;
        }
    }

    public async Task<SimpleHttpResponse<T>?> PostAsync<T>()
    {
        if (_url is null) throw new NullReferenceException("Url has not been set");
        try
        {
            var result = await _httpClient.PostAsync(_url, _body is null ? null : new StringContent(_body));
            var content = await result.Content.ReadAsStringAsync();
            return new SimpleHttpResponse<T>((int)result.StatusCode, content);
        }
        catch (Exception ex)
        {
            Log.Exception(ex);
            return null;
        }
    }

    public async Task<SimpleHttpResponse?> GetAsync()
    {
        if (_url is null) throw new NullReferenceException("Url has not been set");
        try
        {
            var result = await _httpClient.GetAsync(_url);
            var content = await result.Content.ReadAsStringAsync();
            return new SimpleHttpResponse((int)result.StatusCode, content);
        }
        catch (Exception ex)
        {
            Log.Exception(ex);
            return null;
        }
    }

    public async Task<SimpleHttpResponse<T>?> GetAsync<T>()
    {
        if (_url is null) throw new NullReferenceException("Url has not been set");
        try
        {
            var result = await _httpClient.GetAsync(_url);
            var content = await result.Content.ReadAsStringAsync();
            return new SimpleHttpResponse<T>((int)result.StatusCode, content);
        }
        catch (Exception ex)
        {
            Log.Exception(ex);
            return null;
        }
    }
}
