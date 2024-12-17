using System.Net.Http.Headers;

namespace PosClient.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetAuthorizationHeader()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["authToken"];
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
    
    public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
    {
        SetAuthorizationHeader();
        return await _httpClient.PostAsync(url, content);
    }
    public async Task<HttpResponseMessage> PutAsync(string url, HttpContent content)
    {
        SetAuthorizationHeader();
        return await _httpClient.PutAsync(url, content);
    }

    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        SetAuthorizationHeader();
        return await _httpClient.GetAsync(url);
    }
    public async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        SetAuthorizationHeader();
        return await _httpClient.DeleteAsync(url);
    }
}
