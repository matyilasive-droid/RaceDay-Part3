using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[SessionAuthorize]
public class ProfileController : Controller
{
    private readonly IApiClient _api;
    public ProfileController(IApiClient api) => _api = api;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var res = await _api.GetAsync<ProfileDto>(ApiRoutes.Profile);
        if (!res.Success || res.Data is null)
        {
            ViewBag.Error = res.Error;
            var u = HttpContext.Session.GetUser()!;
            return View(new ProfileVm { FirstName = u.FullName.Split(' ', 2)[0], LastName = u.FullName.Contains(' ') ? u.FullName.Split(' ', 2)[1] : "", Email = u.Email });
        }
        var p = res.Data;
        return View(new ProfileVm { FirstName = p.FirstName, LastName = p.LastName, Email = p.Email });
    }

    [HttpPost]
    public async Task<IActionResult> Index(ProfileVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var res = await _api.PutAsync<ProfileDto>(ApiRoutes.Profile, new { vm.FirstName, vm.LastName, vm.Email });
        if (!res.Success)
        {
            ModelState.AddModelError("", res.Error ?? "Could not save your profile.");
            return View(vm);
        }

        var user = HttpContext.Session.GetUser()!;
        HttpContext.Session.SetUser(user with { FullName = $"{vm.FirstName} {vm.LastName}".Trim(), Email = vm.Email });
        TempData["Success"] = "Profile updated.";
        return RedirectToAction(nameof(Index));
    }

}
