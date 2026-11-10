using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddHttpContextAccessor();

// Session holds token, user id and role. Server-side store; cookie only carries the session id.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.Cookie.Name = ".RaceDay.Session";
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    o.IdleTimeout = TimeSpan.FromMinutes(60);
});

// The one and only data source: the Part 2 REST API.
var apiBase = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5000/";
if (!apiBase.EndsWith('/')) apiBase += "/";
builder.Services.AddHttpClient<IApiClient, ApiClient>(c =>
{
    c.BaseAddress = new Uri(apiBase);
    c.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
