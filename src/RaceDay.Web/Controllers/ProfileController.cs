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
            return View(new ProfileVm { FullName = u.FullName, Email = u.Email, ProfileImageUrl = u.ProfileImageUrl });
        }
        var p = res.Data;
        return View(new ProfileVm { FullName = p.FullName, Email = p.Email, PhoneNumber = p.PhoneNumber, DateOfBirth = p.DateOfBirth, ProfileImageUrl = p.ProfileImageUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Index(ProfileVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var res = await _api.PutAsync<ProfileDto>(ApiRoutes.Profile, new { vm.FullName, vm.Email, vm.PhoneNumber, vm.DateOfBirth });
        if (!res.Success)
        {
            ModelState.AddModelError("", res.Error ?? "Could not save your profile.");
            return View(vm);
        }

        var user = HttpContext.Session.GetUser()!;
        HttpContext.Session.SetUser(user with { FullName = vm.FullName, Email = vm.Email });
        TempData["Success"] = "Profile updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UploadPicture(IFormFile? picture)
    {
        var problem = UploadValidator.Validate(picture);
        if (problem is not null)
        {
            TempData["Error"] = problem;
            return RedirectToAction(nameof(Index));
        }

        // Goes to the API, which stores it in Azure Blob Storage and returns the public URL.
        var res = await _api.UploadAsync<UploadResponse>(ApiRoutes.ProfilePicture, picture!);
        if (res.Success && res.Data is not null)
        {
            var user = HttpContext.Session.GetUser()!;
            HttpContext.Session.SetUser(user with { ProfileImageUrl = res.Data.Url });
            TempData["Success"] = "Profile picture updated.";
        }
        else TempData["Error"] = res.Error ?? "Upload failed.";
        return RedirectToAction(nameof(Index));
    }
}
