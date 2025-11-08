using MultitenantPerDb.Core.Infrastructure.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace MultitenantPerDb.Core.Infrastructure.Services.Implementations;

/// <summary>
/// Generic HTTP Client Service Implementation
/// Pure infrastructure - no domain knowledge
/// </summary>
public class HttpClientService : IHttpClientService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpClientService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public HttpClientService(
        IHttpClientFactory httpClientFactory, 
        ILogger<HttpClientService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<TResponse?> GetAsync<TResponse>(string url, Dictionary<string, string>? headers = null)
    {
        var client = _httpClientFactory.CreateClient();
        AddHeaders(client, headers);

        try
        {
            _logger.LogInformation("HTTP GET: {Url}", url);
            
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
            
            _logger.LogInformation("HTTP GET successful: {Url} - Status: {Status}", 
                url, response.StatusCode);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP GET failed: {Url} - Error: {Message}", url, ex.Message);
            throw;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data, Dictionary<string, string>? headers = null)
    {
        var client = _httpClientFactory.CreateClient();
        AddHeaders(client, headers);

        try
        {
            _logger.LogInformation("HTTP POST: {Url}", url);
            
            var response = await client.PostAsJsonAsync(url, data, _jsonOptions);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
            
            _logger.LogInformation("HTTP POST successful: {Url} - Status: {Status}", 
                url, response.StatusCode);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP POST failed: {Url} - Error: {Message}", url, ex.Message);
            throw;
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest data, Dictionary<string, string>? headers = null)
    {
        var client = _httpClientFactory.CreateClient();
        AddHeaders(client, headers);

        try
        {
            _logger.LogInformation("HTTP PUT: {Url}", url);
            
            var response = await client.PutAsJsonAsync(url, data, _jsonOptions);
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
            
            _logger.LogInformation("HTTP PUT successful: {Url} - Status: {Status}", 
                url, response.StatusCode);
            
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP PUT failed: {Url} - Error: {Message}", url, ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string url, Dictionary<string, string>? headers = null)
    {
        var client = _httpClientFactory.CreateClient();
        AddHeaders(client, headers);

        try
        {
            _logger.LogInformation("HTTP DELETE: {Url}", url);
            
            var response = await client.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("HTTP DELETE successful: {Url} - Status: {Status}", 
                url, response.StatusCode);
            
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP DELETE failed: {Url} - Error: {Message}", url, ex.Message);
            return false;
        }
    }

    private void AddHeaders(HttpClient client, Dictionary<string, string>? headers)
    {
        if (headers == null) return;

        foreach (var (key, value) in headers)
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(key, value);
        }
    }
}

/// <summary>
/// Fake HTTP Client Service for Development/Testing
/// </summary>
public class FakeHttpClientService : IHttpClientService
{
    private readonly ILogger<FakeHttpClientService> _logger;

    public FakeHttpClientService(ILogger<FakeHttpClientService> logger)
    {
        _logger = logger;
    }

    public Task<TResponse?> GetAsync<TResponse>(string url, Dictionary<string, string>? headers = null)
    {
        _logger.LogInformation("[FAKE HTTP] GET: {Url}", url);
        
        // Return default value for testing
        return Task.FromResult(default(TResponse));
    }

    public Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data, Dictionary<string, string>? headers = null)
    {
        _logger.LogInformation("[FAKE HTTP] POST: {Url}, Data: {@Data}", url, data);
        
        // Return default value for testing
        return Task.FromResult(default(TResponse));
    }

    public Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest data, Dictionary<string, string>? headers = null)
    {
        _logger.LogInformation("[FAKE HTTP] PUT: {Url}, Data: {@Data}", url, data);
        
        // Return default value for testing
        return Task.FromResult(default(TResponse));
    }

    public Task<bool> DeleteAsync(string url, Dictionary<string, string>? headers = null)
    {
        _logger.LogInformation("[FAKE HTTP] DELETE: {Url}", url);
        
        // Simulate successful deletion
        return Task.FromResult(true);
    }
}
