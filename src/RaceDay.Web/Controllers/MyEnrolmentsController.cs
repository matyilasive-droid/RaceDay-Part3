using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[SessionAuthorize(Role = "Participant")]
public class MyEnrolmentsController : Controller
{
    private readonly IApiClient _api;
    public MyEnrolmentsController(IApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var res = await _api.GetAsync<List<EnrolmentDto>>(ApiRoutes.MyEnrolments);
        if (!res.Success) ViewBag.Error = res.Error;
        return View((res.Data ?? new()).OrderBy(e => e.EventDate).ToList());
    }
}
