namespace #@Namespace#.Services;
public class BaseService
{
    protected HttpClient Http { get; init; }
    public JsonSerializerOptions JsonSerializerOptions { get; set; }
    public ErrorResult? ErrorMsg { get; set; }

    public BaseService(IHttpClientFactory httpClient)
    {
        Http = httpClient.CreateClient("#@Namespace#");
        JsonSerializerOptions = new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };
    }

    /// <summary>
    /// json post 封装
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="route"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    protected async Task<TResult?> PostJsonAsync<TResult>(string route, object? data = null)
    {
        return await SendJsonAsync<TResult>(HttpMethod.Post, route, data);
    }

    /// <summary>
    /// Put
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="route"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    protected async Task<TResult?> PutJsonAsync<TResult>(string route, object? data = null)
    {
        return await SendJsonAsync<TResult>(HttpMethod.Put, route, data);
    }

    protected async Task<TResult?> PatchJsonAsync<TResult>(string route, object? data = null)
    {
        return await SendJsonAsync<TResult>(HttpMethod.Patch, route, data);
    }

    /// <summary>
    /// get
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="route"></param>
    /// <param name="dic"></param>
    /// <returns></returns>
    protected async Task<TResult?> GetJsonAsync<TResult>(string route, Dictionary<string, string?>? dic = null)
    {
        return await SendJsonAsync<TResult>(HttpMethod.Get, route, dic);
    }

    /// <summary>
    /// delete
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="route"></param>
    /// <param name="dic"></param>
    /// <returns></returns>
    protected async Task<TResult?> DeleteJsonAsync<TResult>(string route, Dictionary<string, string?>? dic = null)
    {
        return await SendJsonAsync<TResult>(HttpMethod.Delete, route, dic);
    }

    // upload file
    protected async Task<TResult?> UploadFileAsync<TResult>(string route, StreamContent file)
    {
        HttpResponseMessage? res = await Http.PostAsync(route, new MultipartFormDataContent
        {
            { file, "file", "file" }
        });
        if (res != null && res.IsSuccessStatusCode)
        {
            return await res.Content.ReadFromJsonAsync<TResult>();
        }
        else
        {
            try
            {
                ErrorMsg = await res!.Content.ReadFromJsonAsync<ErrorResult>();
            }
            catch (Exception ex)
            {
                var content = await res!.Content.ReadAsStringAsync();
                ErrorMsg = new ErrorResult
                {
                    Title = ex.Message,
                    Detail = content,
                };
                return default;
            }

            return default;
        }
    }

    // download file
    protected async Task<Stream?> DownloadFileAsync(string route)
    {
        HttpResponseMessage? res = await Http.GetAsync(route);
        if (res != null && res.IsSuccessStatusCode)
        {
            return await res.Content.ReadAsStreamAsync();
        }
        else
        {
            try
            {
                ErrorMsg = await res!.Content.ReadFromJsonAsync<ErrorResult>();
            }
            catch (Exception ex)
            {
                var content = await res!.Content.ReadAsStringAsync();
                ErrorMsg = new ErrorResult
                {
                    Title = ex.Message,
                    Detail = content,
                };
                return default;
            }

            return default;
        }
    }

    protected static string ToUrlParameters(Dictionary<string, string?> dic)
    {
        return string.Join("&", dic.Where(d => d.Value != null)
            .Select(d => string.Format("{0}={1}", d.Key, d.Value))
            );
    }

    protected async Task<TResult?> SendJsonAsync<TResult>(HttpMethod method, string route, object? data)
    {
        route = Http.BaseAddress + (route.StartsWith('/') ? route[1..] : route);
        HttpResponseMessage? res = null;
        if (method == HttpMethod.Post)
        {
            res = await Http.PostAsJsonAsync(route, data, JsonSerializerOptions);
        }
        else if (method == HttpMethod.Put)
        {
            res = await Http.PutAsJsonAsync(route, data, JsonSerializerOptions);
        }
        else if (method == HttpMethod.Patch)
        {
            res = await Http.PatchAsJsonAsync(route, data, JsonSerializerOptions);
        }
        if (res != null && res.IsSuccessStatusCode)
        {
            return await res.Content.ReadFromJsonAsync<TResult>();
        }
        else
        {
            try
            {
                ErrorMsg = await res!.Content.ReadFromJsonAsync<ErrorResult>();
            }
            catch (Exception ex)
            {
                var content = await res!.Content.ReadAsStringAsync();
                ErrorMsg = new ErrorResult
                {
                    Title = ex.Message,
                    Detail = content,
                };
                return default;
            }

            return default;
        }
    }

    protected async Task<TResult?> SendJsonAsync<TResult>(HttpMethod method, string route, Dictionary<string, string?>? dic = null)
    {
        route = Http.BaseAddress + (route.StartsWith('/') ? route[1..] : route);
        if (dic != null)
        {
            route = route + "?" + ToUrlParameters(dic);
        }
        HttpResponseMessage? res = null;
        if (method == HttpMethod.Get)
        {
            res = await Http.GetAsync(route);

        }
        else if (method == HttpMethod.Delete)
        {
            res = await Http.DeleteAsync(route);

        }
        if (res != null && res.IsSuccessStatusCode)
        {
            return await res.Content.ReadFromJsonAsync<TResult>();
        }
        else
        {
            try
            {
                ErrorMsg = await res!.Content.ReadFromJsonAsync<ErrorResult>();
            }
            catch (Exception ex)
            {
                var content = await res!.Content.ReadAsStringAsync();
                ErrorMsg = new ErrorResult
                {
                    Title = ex.Message,
                    Detail = content,
                };
                return default;
            }

            return default;
        }
    }
}

public class ErrorResult
{
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public int Number { get; set; }
}