using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RaceDay.Web.Services;

public record ApiResult<T>(bool Success, T? Data, string? Error, int StatusCode);

public interface IApiClient
{
    Task<ApiResult<T>> GetAsync<T>(string url);
    Task<ApiResult<T>> PostAsync<T>(string url, object? body);
    Task<ApiResult<T>> PutAsync<T>(string url, object? body);
    Task<ApiResult<bool>> DeleteAsync(string url);
    Task<ApiResult<T>> UploadAsync<T>(string url, IFormFile file, string fieldName = "file");
}

/// <summary>
/// The ONLY way the MVC app talks to data. Every call goes over HTTP to the Part 2 API,
/// attaching the JWT held in the user's session. No database or Blob Storage access exists here.
/// </summary>
public class ApiClient : IApiClient
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private readonly HttpClient _client;
    private readonly IHttpContextAccessor _http;

    public ApiClient(HttpClient client, IHttpContextAccessor http)
    {
        _client = client;
        _http = http;
    }

    public Task<ApiResult<T>> GetAsync<T>(string url) => SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, url));

    public Task<ApiResult<T>> PostAsync<T>(string url, object? body) =>
        SendAsync<T>(new HttpRequestMessage(HttpMethod.Post, url) { Content = ToJson(body) });

    public Task<ApiResult<T>> PutAsync<T>(string url, object? body) =>
        SendAsync<T>(new HttpRequestMessage(HttpMethod.Put, url) { Content = ToJson(body) });

    public async Task<ApiResult<bool>> DeleteAsync(string url)
    {
        var r = await SendAsync<object>(new HttpRequestMessage(HttpMethod.Delete, url));
        return new ApiResult<bool>(r.Success, r.Success, r.Error, r.StatusCode);
    }

    public async Task<ApiResult<T>> UploadAsync<T>(string url, IFormFile file, string fieldName = "file")
    {
        await using var stream = file.OpenReadStream();
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
        content.Add(fileContent, fieldName, file.FileName);
        return await SendAsync<T>(new HttpRequestMessage(HttpMethod.Post, url) { Content = content });
    }

    private static StringContent ToJson(object? body) =>
        new(JsonSerializer.Serialize(body ?? new { }, Json), Encoding.UTF8, "application/json");

    private async Task<ApiResult<T>> SendAsync<T>(HttpRequestMessage request)
    {
        var user = _http.HttpContext?.Session.GetUser();
        if (user is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

        try
        {
            using var response = await _client.SendAsync(request);
            var text = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new ApiResult<T>(false, default, ExtractError(text, (int)response.StatusCode), (int)response.StatusCode);

            T? data = default;
            if (!string.IsNullOrWhiteSpace(text))
            {
                try { data = JsonSerializer.Deserialize<T>(text, Json); }
                catch (JsonException)
                {
                    return new ApiResult<T>(false, default, "The server returned an unexpected response.", 502);
                }
            }
            return new ApiResult<T>(true, data, null, (int)response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return new ApiResult<T>(false, default, "Cannot reach the RaceDay service. Please try again shortly.", 503);
        }
        catch (TaskCanceledException)
        {
            return new ApiResult<T>(false, default, "The RaceDay service took too long to respond.", 504);
        }
    }

    /// <summary>Turns an API error body into a friendly message (never a raw exception).</summary>
    public static string ExtractError(string? body, int status)
    {
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var name in new[] { "message", "error", "detail", "title" })
                        foreach (var prop in doc.RootElement.EnumerateObject())
                            if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase)
                                && prop.Value.ValueKind == JsonValueKind.String
                                && !string.IsNullOrWhiteSpace(prop.Value.GetString()))
                                return prop.Value.GetString()!;
                }
            }
            catch (JsonException)
            {
                var trimmed = body.Trim();
                if (trimmed.Length <= 200 && !trimmed.StartsWith('<')) return trimmed;
            }
        }

        return status switch
        {
            400 => "The request was not valid. Please check your details.",
            401 => "Your session has expired or your credentials are incorrect.",
            403 => "You do not have permission to do that.",
            404 => "We could not find what you were looking for.",
            409 => "That conflicts with existing data (for example, you may already be entered).",
            >= 500 => "The RaceDay service hit a problem. Please try again.",
            _ => "Something went wrong. Please try again."
        };
    }
}
