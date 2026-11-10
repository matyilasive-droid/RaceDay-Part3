using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Tests;

public class ApiClientTests
{
    private class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;
        public HttpRequestMessage? Last { get; private set; }
        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) => _respond = respond;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Last = request;
            return Task.FromResult(_respond(request));
        }
    }

    private static HttpResponseMessage Json(HttpStatusCode code, string json) =>
        new(code) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static ApiClient Make(StubHandler h, HttpContext? ctx = null) =>
        new(new HttpClient(h) { BaseAddress = new Uri("http://api.test/") }, new HttpContextAccessor { HttpContext = ctx });

    [Fact]
    public async Task Get_deserialises_camelCase_json()
    {
        var h = new StubHandler(_ => Json(HttpStatusCode.OK, "[{\"id\":7,\"name\":\"Park 10K\",\"distanceKm\":10}]"));
        var r = await Make(h).GetAsync<List<EventDto>>("api/events");
        Assert.True(r.Success);
        Assert.Equal("Park 10K", r.Data![0].Name);
        Assert.Equal(7, r.Data[0].Id);
    }

    [Fact]
    public async Task Bearer_token_is_attached_from_session()
    {
        var h = new StubHandler(_ => Json(HttpStatusCode.OK, "[]"));
        await Make(h, Ctx.WithSession(Ctx.Participant())).GetAsync<List<EventDto>>("api/events");
        Assert.Equal("Bearer", h.Last!.Headers.Authorization!.Scheme);
        Assert.Equal("tok", h.Last.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task No_auth_header_when_anonymous()
    {
        var h = new StubHandler(_ => Json(HttpStatusCode.OK, "[]"));
        await Make(h, Ctx.WithSession()).GetAsync<List<EventDto>>("api/events");
        Assert.Null(h.Last!.Headers.Authorization);
    }

    [Fact]
    public async Task Error_message_is_taken_from_api_body()
    {
        var h = new StubHandler(_ => Json(HttpStatusCode.Conflict, "{\"message\":\"Already entered\"}"));
        var r = await Make(h).PostAsync<object>("api/events/1/enrol", new { });
        Assert.False(r.Success);
        Assert.Equal(409, r.StatusCode);
        Assert.Equal("Already entered", r.Error);
    }

    [Fact]
    public async Task Server_error_returns_friendly_text_not_exception()
    {
        var h = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("<html>stack trace</html>") });
        var r = await Make(h).GetAsync<EventDto>("api/events/1");
        Assert.False(r.Success);
        Assert.DoesNotContain("<html>", r.Error);
    }

    [Fact]
    public async Task Unreachable_api_returns_503()
    {
        var h = new StubHandler(_ => throw new HttpRequestException("boom"));
        var r = await Make(h).GetAsync<EventDto>("api/events/1");
        Assert.False(r.Success);
        Assert.Equal(503, r.StatusCode);
    }

    [Fact]
    public async Task Malformed_json_is_handled()
    {
        var h = new StubHandler(_ => Json(HttpStatusCode.OK, "not json"));
        var r = await Make(h).GetAsync<EventDto>("api/events/1");
        Assert.False(r.Success);
    }

    [Fact]
    public async Task Delete_reports_success()
    {
        var h = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var r = await Make(h).DeleteAsync("api/events/1");
        Assert.True(r.Success);
        Assert.Equal(HttpMethod.Delete, h.Last!.Method);
    }

    [Fact]
    public async Task Upload_sends_multipart_form_data()
    {
        var h = new StubHandler(_ => Json(HttpStatusCode.OK, "{\"url\":\"https://blob/x.png\"}"));
        var file = new FormFile(new MemoryStream(new byte[] { 1, 2, 3 }), 0, 3, "file", "x.png") { Headers = new HeaderDictionary(), ContentType = "image/png" };
        var r = await Make(h).UploadAsync<UploadResponse>("api/events/1/banner", file);
        Assert.True(r.Success);
        Assert.Equal("https://blob/x.png", r.Data!.Url);
        Assert.StartsWith("multipart/form-data", h.Last!.Content!.Headers.ContentType!.MediaType!);
    }
}
