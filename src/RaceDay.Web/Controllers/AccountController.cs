using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class AccountController : Controller
{
    private readonly IApiClient _api;
    public AccountController(IApiClient api) => _api = api;

    [HttpGet]
    public IActionResult Login(string? returnUrl = null, bool loggedOut = false)
    {
        if (HttpContext.Session.GetUser() is { } user) return RedirectToHome(user.Role);
        ViewBag.LoggedOut = loggedOut;
        return View(new LoginVm { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _api.PostAsync<AuthResponse>(ApiRoutes.Login, new { vm.Email, vm.Password });
        if (!result.Success || result.Data is null || string.IsNullOrEmpty(result.Data.Token))
        {
            ModelState.AddModelError("", result.StatusCode is 400 or 401
                ? "Invalid email or password."
                : result.Error ?? "Sign-in failed. Please try again.");
            return View(vm);
        }

        var auth = result.Data;
        HttpContext.Session.Clear(); // fresh session on login
        HttpContext.Session.SetUser(new SessionUser(auth.Token, auth.UserId, auth.FullName, auth.Email, auth.Role, auth.ProfileImageUrl));

        if (Format.IsLocalUrl(vm.ReturnUrl)) return LocalRedirect(vm.ReturnUrl!);
        return RedirectToHome(auth.Role);
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterVm());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (vm.Role is not ("Participant" or "Organiser"))
            ModelState.AddModelError(nameof(vm.Role), "Please choose a valid account type.");
        if (!ModelState.IsValid) return View(vm);

        var result = await _api.PostAsync<object>(ApiRoutes.Register,
            new { vm.FullName, vm.Email, vm.Password, vm.Role });
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error ?? "Registration failed.");
            return View(vm);
        }

        TempData["Success"] = "Account created. Sign in to get started.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login), new { loggedOut = true });
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    private IActionResult RedirectToHome(string role) =>
        role.Equals("Organiser", StringComparison.OrdinalIgnoreCase)
            ? RedirectToAction("Dashboard", "Organiser")
            : RedirectToAction("Index", "Home");
}
