using System.Net.Http.Json;

namespace Ater.AspNetCore.Utils;

public class SimpleHttp
{
    public HttpClient client = new();
    public int StatusCode { get; private set; }
    public string? ResponseContent { get; private set; }

    public SimpleHttp() { }

    /// <summary>
    /// json post请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public async Task<T?> PostJsonAsync<T>(string url, object data)
    {
        client.Timeout = TimeSpan.FromSeconds(5);
        HttpResponseMessage res = await client.PostAsJsonAsync(url, data);
        if (res.IsSuccessStatusCode)
        {
            return await res.Content.ReadFromJsonAsync<T>();
        }
        else
        {
            StatusCode = (int)res.StatusCode;
            ResponseContent = await res.Content.ReadAsStringAsync();
            return default;
        }
    }
}
