using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Helpers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[SessionAuthorize(Role = "Organiser")]
public class OrganiserController : Controller
{
    private readonly IApiClient _api;
    public OrganiserController(IApiClient api) => _api = api;

    // ---------- Dashboard ----------
    public async Task<IActionResult> Dashboard()
    {
        var res = await _api.GetAsync<List<EventDto>>(ApiRoutes.MyEvents);
        if (!res.Success) ViewBag.Error = res.Error;
        return View((res.Data ?? new()).OrderBy(e => e.Date).ToList());
    }

    // ---------- Events CRUD ----------
    [HttpGet]
    public IActionResult CreateEvent() => View("EventForm", new EventFormVm());

    [HttpPost]
    public async Task<IActionResult> CreateEvent(EventFormVm vm)
    {
        if (vm.Banner is not null) ValidateBanner(vm);
        if (!ModelState.IsValid) return View("EventForm", vm);

        var res = await _api.PostAsync<EventDto>(ApiRoutes.Events, ToBody(vm));
        if (!res.Success || res.Data is null)
        {
            ModelState.AddModelError("", res.Error ?? "Could not create the event.");
            return View("EventForm", vm);
        }

        var bannerError = await UploadBannerAsync(res.Data.Id, vm.Banner);
        TempData["Success"] = "Event created.";
        if (bannerError is not null) TempData["Error"] = "Event saved, but the banner failed: " + bannerError;
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        var res = await _api.GetAsync<EventDto>(ApiRoutes.Event(id));
        if (!res.Success || res.Data is null)
        {
            TempData["Error"] = res.Error ?? "Event not found.";
            return RedirectToAction(nameof(Dashboard));
        }
        var e = res.Data;
        return View("EventForm", new EventFormVm
        {
            Id = e.Id, Name = e.Name, Description = e.Description, Date = e.Date,
            Location = e.Location, DistanceKm = e.DistanceKm, EventType = e.EventType, BannerUrl = e.BannerUrl
        });
    }

    [HttpPost]
    public async Task<IActionResult> EditEvent(int id, EventFormVm vm)
    {
        vm.Id = id;
        if (vm.Banner is not null) ValidateBanner(vm);
        if (!ModelState.IsValid) return View("EventForm", vm);

        var res = await _api.PutAsync<EventDto>(ApiRoutes.Event(id), ToBody(vm));
        if (!res.Success)
        {
            ModelState.AddModelError("", res.Error ?? "Could not update the event.");
            return View("EventForm", vm);
        }

        var bannerError = await UploadBannerAsync(id, vm.Banner);
        TempData["Success"] = "Event updated.";
        if (bannerError is not null) TempData["Error"] = "Event saved, but the banner failed: " + bannerError;
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var res = await _api.DeleteAsync(ApiRoutes.Event(id));
        if (res.Success) TempData["Success"] = "Event deleted.";
        else TempData["Error"] = res.Error ?? "Could not delete the event.";
        return RedirectToAction(nameof(Dashboard));
    }

    // ---------- Categories ----------
    [HttpGet]
    public async Task<IActionResult> Categories(int id)
    {
        var vm = await LoadPageAsync(id);
        if (vm is null) return RedirectToAction(nameof(Dashboard));
        var cats = await _api.GetAsync<List<CategoryDto>>(ApiRoutes.EventCategories(id));
        vm.Categories = cats.Data ?? new();
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> AddCategory(int id, CategoryFormVm form)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please give the category a name.";
            return RedirectToAction(nameof(Categories), new { id });
        }
        var res = await _api.PostAsync<CategoryDto>(ApiRoutes.EventCategories(id), new { form.Name, form.Type });
        if (res.Success) TempData["Success"] = "Category added.";
        else TempData["Error"] = res.Error ?? "Could not add the category.";
        return RedirectToAction(nameof(Categories), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteCategory(int id, int categoryId)
    {
        var res = await _api.DeleteAsync(ApiRoutes.EventCategory(id, categoryId));
        if (res.Success) TempData["Success"] = "Category removed.";
        else TempData["Error"] = res.Error ?? "Could not remove the category.";
        return RedirectToAction(nameof(Categories), new { id });
    }

    // ---------- Enrolments ----------
    [HttpGet]
    public async Task<IActionResult> Enrolments(int id)
    {
        var vm = await LoadPageAsync(id);
        if (vm is null) return RedirectToAction(nameof(Dashboard));
        var res = await _api.GetAsync<List<EnrolmentDto>>(ApiRoutes.EventEnrolments(id));
        if (!res.Success) ViewBag.Error = res.Error;
        vm.Enrolments = res.Data ?? new();
        return View(vm);
    }

    // ---------- Results ----------
    [HttpGet]
    public async Task<IActionResult> Results(int id)
    {
        var vm = await LoadPageAsync(id);
        if (vm is null) return RedirectToAction(nameof(Dashboard));
        var enr = await _api.GetAsync<List<EnrolmentDto>>(ApiRoutes.EventEnrolments(id));
        var res = await _api.GetAsync<List<ResultDto>>(ApiRoutes.EventResults(id));
        if (!enr.Success) ViewBag.Error = enr.Error;
        vm.Enrolments = (enr.Data ?? new()).Where(e => !e.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)).ToList();
        vm.Results = res.Data ?? new();
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> SaveResult(int id, int enrolmentId, string finishTime, int position)
    {
        if (!TimeSpan.TryParse(finishTime, out var time) || time <= TimeSpan.Zero)
        {
            TempData["Error"] = "Finish time must look like 01:23:45 (hh:mm:ss).";
            return RedirectToAction(nameof(Results), new { id });
        }
        if (position < 1)
        {
            TempData["Error"] = "Position must be 1 or higher.";
            return RedirectToAction(nameof(Results), new { id });
        }

        var res = await _api.PostAsync<ResultDto>(ApiRoutes.EventResults(id), new { enrolmentId, finishTime = time, position });
        if (res.Success) TempData["Success"] = "Result saved.";
        else TempData["Error"] = res.Error ?? "Could not save the result.";
        return RedirectToAction(nameof(Results), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> PublishResults(int id)
    {
        var res = await _api.PostAsync<object>(ApiRoutes.EventResultsPublish(id), null);
        if (res.Success) TempData["Success"] = "Results published to participants.";
        else TempData["Error"] = res.Error ?? "Could not publish results.";
        return RedirectToAction(nameof(Results), new { id });
    }

    // ---------- helpers ----------
    private async Task<EventPageVm?> LoadPageAsync(int id)
    {
        var ev = await _api.GetAsync<EventDto>(ApiRoutes.Event(id));
        if (!ev.Success || ev.Data is null)
        {
            TempData["Error"] = ev.Error ?? "Event not found.";
            return null;
        }
        return new EventPageVm { Event = ev.Data };
    }

    private void ValidateBanner(EventFormVm vm)
    {
        var problem = UploadValidator.Validate(vm.Banner);
        if (problem is not null) ModelState.AddModelError(nameof(vm.Banner), problem);
    }

    private async Task<string?> UploadBannerAsync(int eventId, IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;
        var r = await _api.UploadAsync<UploadResponse>(ApiRoutes.EventBanner(eventId), file);
        return r.Success ? null : r.Error ?? "Upload failed.";
    }

    private static object ToBody(EventFormVm vm) =>
        new { vm.Name, vm.Description, vm.Date, vm.Location, vm.DistanceKm, vm.EventType };
}
