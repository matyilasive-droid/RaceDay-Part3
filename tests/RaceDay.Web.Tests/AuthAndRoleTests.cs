using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using RaceDay.Web.Controllers;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Tests;

public class AuthAndRoleTests
{
    private static AuthorizationFilterContext FilterCtx(Microsoft.AspNetCore.Http.HttpContext http) =>
        new(new ActionContext(http, new RouteData(), new ActionDescriptor()), new List<IFilterMetadata>());

    // ---- SessionAuthorize filter ----
    [Fact]
    public void Anonymous_user_is_redirected_to_login()
    {
        var ctx = FilterCtx(Ctx.WithSession());
        new SessionAuthorizeAttribute { Role = "Organiser" }.OnAuthorization(ctx);
        var redirect = Assert.IsType<RedirectToActionResult>(ctx.Result);
        Assert.Equal("Login", redirect.ActionName);
    }

    [Fact]
    public void Wrong_role_is_sent_to_access_denied()
    {
        var ctx = FilterCtx(Ctx.WithSession(Ctx.Participant()));
        new SessionAuthorizeAttribute { Role = "Organiser" }.OnAuthorization(ctx);
        Assert.Equal("AccessDenied", Assert.IsType<RedirectToActionResult>(ctx.Result).ActionName);
    }

    [Fact]
    public void Correct_role_passes()
    {
        var ctx = FilterCtx(Ctx.WithSession(Ctx.Organiser()));
        new SessionAuthorizeAttribute { Role = "Organiser" }.OnAuthorization(ctx);
        Assert.Null(ctx.Result);
    }

    [Fact]
    public void Any_logged_in_user_passes_when_no_role_required()
    {
        var ctx = FilterCtx(Ctx.WithSession(Ctx.Participant()));
        new SessionAuthorizeAttribute().OnAuthorization(ctx);
        Assert.Null(ctx.Result);
    }

    // ---- Controllers carry the right protection ----
    [Theory]
    [InlineData(typeof(OrganiserController), "Organiser")]
    [InlineData(typeof(MyEnrolmentsController), "Participant")]
    [InlineData(typeof(MyResultsController), "Participant")]
    [InlineData(typeof(ProfileController), null)]
    public void Protected_controllers_declare_expected_role(Type controller, string? role)
    {
        var attr = (SessionAuthorizeAttribute?)Attribute.GetCustomAttribute(controller, typeof(SessionAuthorizeAttribute));
        Assert.NotNull(attr);
        Assert.Equal(role, attr!.Role);
    }

    [Fact]
    public void Enter_action_requires_participant()
    {
        var m = typeof(EventsController).GetMethod(nameof(EventsController.Enter))!;
        var attr = (SessionAuthorizeAttribute?)Attribute.GetCustomAttribute(m, typeof(SessionAuthorizeAttribute));
        Assert.Equal("Participant", attr?.Role);
    }

    // ---- AccountController ----
    private static AccountController MakeAccount(FakeApiClient api, SessionUser? user = null)
    {
        var c = new AccountController(api) { ControllerContext = new ControllerContext { HttpContext = Ctx.WithSession(user) } };
        return c;
    }

    private static AuthResponse Auth(string role) => new() { Token = "jwt", UserId = "42", FullName = "Sam", Email = "s@x.com", Role = role };

    [Fact]
    public async Task Login_stores_user_in_session_and_redirects_organiser_to_dashboard()
    {
        var api = new FakeApiClient();
        api.Responses[ApiRoutes.Login] = Auth("Organiser");
        var c = MakeAccount(api);
        var result = await c.Login(new LoginVm { Email = "s@x.com", Password = "Password1" });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Dashboard", redirect.ActionName);
        var stored = c.HttpContext.Session.GetUser();
        Assert.Equal("jwt", stored!.Token);
        Assert.Equal("42", stored.UserId);
        Assert.True(stored.IsOrganiser);
    }

    [Fact]
    public async Task Login_redirects_participant_to_home()
    {
        var api = new FakeApiClient();
        api.Responses[ApiRoutes.Login] = Auth("Participant");
        var result = await MakeAccount(api).Login(new LoginVm { Email = "s@x.com", Password = "Password1" });
        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(result).ActionName);
    }

    [Fact]
    public async Task Login_failure_shows_friendly_error_and_no_session()
    {
        var api = new FakeApiClient();
        api.Errors[ApiRoutes.Login] = (401, "raw");
        var c = MakeAccount(api);
        var result = await c.Login(new LoginVm { Email = "s@x.com", Password = "bad" });

        Assert.IsType<ViewResult>(result);
        Assert.Null(c.HttpContext.Session.GetUser());
        Assert.Contains(c.ModelState[""]!.Errors, e => e.ErrorMessage == "Invalid email or password.");
    }

    [Fact]
    public async Task Login_ignores_external_return_url()
    {
        var api = new FakeApiClient();
        api.Responses[ApiRoutes.Login] = Auth("Participant");
        var result = await MakeAccount(api).Login(new LoginVm { Email = "s@x.com", Password = "Password1", ReturnUrl = "https://evil.com" });
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task Invalid_model_does_not_call_api()
    {
        var api = new FakeApiClient();
        var c = MakeAccount(api);
        c.ModelState.AddModelError("Email", "required");
        await c.Login(new LoginVm());
        Assert.Empty(api.Calls);
    }

    [Fact]
    public void Logout_clears_session_and_redirects_to_login()
    {
        var c = MakeAccount(new FakeApiClient(), Ctx.Participant());
        Assert.NotNull(c.HttpContext.Session.GetUser());
        var result = c.Logout();
        Assert.Null(c.HttpContext.Session.GetUser());
        Assert.Equal("Login", Assert.IsType<RedirectToActionResult>(result).ActionName);
    }

    [Fact]
    public async Task Register_rejects_unknown_role()
    {
        var api = new FakeApiClient();
        var result = await MakeAccount(api).Register(new RegisterVm { FullName = "A", Email = "a@x.com", Password = "Password1", ConfirmPassword = "Password1", Role = "Admin" });
        Assert.IsType<ViewResult>(result);
        Assert.Empty(api.Calls);
    }
}
