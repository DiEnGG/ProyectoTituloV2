// Services/IApiService.cs
using System.Text.Json;
using System.Text;
using WebApp.Models;

public interface IApiService
{
    Task<List<T>> GetAsync<T>(string endpoint);
    Task<T> GetByIdAsync<T>(string endpoint, int id);
    Task<bool> PostAsync<T>(string endpoint, T data);
    Task<LoginResponse> LoginPostAsync<LoginResponse>(string endpoint, LoginViewModel data);
    Task<bool> PutAsync<T>(string endpoint, T data);
    //Task<bool> DeleteAsync(string endpoint, int id);
}

// Services/ApiService.cs
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<T>> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<List<T>>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<T> GetByIdAsync<T>(string endpoint, int id)
    {
        var response = await _httpClient.GetAsync($"{endpoint}/{id}");
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<bool> PostAsync<T>(string endpoint, T data)
    {
        var json = JsonSerializer.Serialize(data);
        var response = await _httpClient.PostAsync(endpoint, new StringContent(json, Encoding.UTF8, "application/json"));  

        return response.IsSuccessStatusCode;
    }

    public async Task<LoginResponse> LoginPostAsync<LoginResponse>(string endpoint, LoginViewModel data)
    {
        var json = JsonSerializer.Serialize(data);
        var response = await _httpClient.PostAsync(endpoint, new StringContent(json, Encoding.UTF8, "application/json"));

        var responseContent = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<bool> PutAsync<T>(string endpoint, T data)
    {
        var json = JsonSerializer.Serialize(data);
        var response = await _httpClient.PutAsync($"{endpoint}/", new StringContent(json, Encoding.UTF8, "application/json"));
        return response.IsSuccessStatusCode;
    }
}
