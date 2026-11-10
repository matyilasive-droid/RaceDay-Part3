using Microsoft.AspNetCore.Http;
using RaceDay.Web.Services;

namespace RaceDay.Web.Tests;

public class TestSession : ISession
{
    private readonly Dictionary<string, byte[]> _store = new();
    public bool IsAvailable => true;
    public string Id { get; } = Guid.NewGuid().ToString();
    public IEnumerable<string> Keys => _store.Keys;
    public void Clear() => _store.Clear();
    public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken ct = default) => Task.CompletedTask;
    public void Remove(string key) => _store.Remove(key);
    public void Set(string key, byte[] value) => _store[key] = value;
    public bool TryGetValue(string key, out byte[]? value)
    {
        var found = _store.TryGetValue(key, out var v);
        value = v;
        return found;
    }
}

public class FakeApiClient : IApiClient
{
    public Dictionary<string, object?> Responses { get; } = new();
    public Dictionary<string, (int Status, string Error)> Errors { get; } = new();
    public List<string> Calls { get; } = new();

    private ApiResult<T> Make<T>(string method, string url)
    {
        Calls.Add($"{method} {url}");
        if (Errors.TryGetValue(url, out var err)) return new ApiResult<T>(false, default, err.Error, err.Status);
        if (Responses.TryGetValue(url, out var v)) return new ApiResult<T>(true, (T?)v, null, 200);
        return new ApiResult<T>(false, default, "not found", 404);
    }

    public Task<ApiResult<T>> GetAsync<T>(string url) => Task.FromResult(Make<T>("GET", url));
    public Task<ApiResult<T>> PostAsync<T>(string url, object? body) => Task.FromResult(Make<T>("POST", url));
    public Task<ApiResult<T>> PutAsync<T>(string url, object? body) => Task.FromResult(Make<T>("PUT", url));
    public Task<ApiResult<bool>> DeleteAsync(string url)
    {
        Calls.Add($"DELETE {url}");
        return Task.FromResult(new ApiResult<bool>(!Errors.ContainsKey(url), true, null, 200));
    }
    public Task<ApiResult<T>> UploadAsync<T>(string url, IFormFile file, string fieldName = "file") => Task.FromResult(Make<T>("UPLOAD", url));
}

public static class Ctx
{
    public static DefaultHttpContext WithSession(SessionUser? user = null)
    {
        var ctx = new DefaultHttpContext { Session = new TestSession() };
        if (user is not null) ctx.Session.SetUser(user);
        return ctx;
    }

    public static SessionUser Participant() => new("tok", "u1", "Pat Participant", "pat@x.com", "Participant", null);
    public static SessionUser Organiser() => new("tok", "u2", "Olly Organiser", "olly@x.com", "Organiser", null);
}
