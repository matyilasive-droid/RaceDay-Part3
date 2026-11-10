using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[SessionAuthorize(Role = "Participant")]
public class MyResultsController : Controller
{
    private readonly IApiClient _api;
    public MyResultsController(IApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var res = await _api.GetAsync<List<ResultDto>>(ApiRoutes.MyResults);
        if (!res.Success) ViewBag.Error = res.Error;
        return View((res.Data ?? new()).OrderByDescending(r => r.EventDate).ToList());
    }
}
