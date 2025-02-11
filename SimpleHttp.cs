using System.Net.Http.Json;
using System.Text;
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
    private string _contentType = "application/json";

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

    public SimpleHttp WithBody(string value, string contentType)
    {
        _body = value;
        _contentType = contentType;
        return this;
    }

    public SimpleHttp WithBody<T>(T value)
    {
        _body = JsonConvert.SerializeObject(value);
        _contentType = "application/json";
        return this;
    }

    public SimpleHttp WithContentType(string contentType)
    {
        _contentType = contentType;
        return this;
    }

    public async Task<SimpleHttpResponse?> PutAsync()
    {
        if (_url is null) throw new NullReferenceException("Url has not been set");
        try
        {
            var result = await _httpClient.PutAsync(_url, _body is null ? null : new StringContent(_body, Encoding.UTF8, _contentType));
            var content = await result.Content.ReadAsStringAsync();
            return new SimpleHttpResponse((int)result.StatusCode, content);
        }
        catch (Exception ex)
        {
            Log.Exception(ex);
            return null;
        }
    }

    public async Task<SimpleHttpResponse<T>?> PutAsync<T>()
    {
        if (_url is null) throw new NullReferenceException("Url has not been set");
        try
        {
            var result = await _httpClient.PutAsync(_url, _body is null ? null : new StringContent(_body, Encoding.UTF8, _contentType));
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
