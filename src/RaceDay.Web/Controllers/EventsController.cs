using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class EventsController : Controller
{
    private readonly IApiClient _api;
    public EventsController(IApiClient api) => _api = api;

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var ev = await _api.GetAsync<EventDto>(ApiRoutes.Event(id));
        if (!ev.Success || ev.Data is null)
        {
            TempData["Error"] = ev.Error ?? "Event not found.";
            return RedirectToAction("Index", "Home");
        }
        var cats = await _api.GetAsync<List<CategoryDto>>(ApiRoutes.EventCategories(id));
        return View(new EventDetailsVm { Event = ev.Data, Categories = cats.Data ?? ev.Data.Categories });
    }

    [HttpPost, SessionAuthorize(Role = "Participant")]
    public async Task<IActionResult> Enter(int id, int? categoryId)
    {
        var res = await _api.PostAsync<EnrolmentDto>(ApiRoutes.EventEnrol(id), new { CategoryID = categoryId });
        if (res.Success)
        {
            TempData["Success"] = "You're in! Your entry has been submitted.";
            return RedirectToAction("Index", "MyEnrolments");
        }
        TempData["Error"] = res.Error ?? "Could not enter this event.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
