using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class HomeController : Controller
{
    private readonly IApiClient _api;
    public HomeController(IApiClient api) => _api = api;

    public async Task<IActionResult> Index(string? search, string? type, string? category)
    {
        var res = await _api.GetAsync<List<EventDto>>(ApiRoutes.Events);
        var all = (res.Data ?? new()).Where(e => e.Date >= DateTime.Today).OrderBy(e => e.Date).ToList();

        var vm = new HomeVm
        {
            Search = search, Type = type, Category = category,
            Error = res.Success ? null : res.Error,
            Types = all.Select(e => e.EventType).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().OrderBy(t => t).ToList(),
            Categories = all.SelectMany(e => e.Categories).Select(c => c.Name).Distinct().OrderBy(n => n).ToList()
        };

        IEnumerable<EventDto> q = all;
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                          || e.Location.Contains(search, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(type))
            q = q.Where(e => e.EventType.Equals(type, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(e => e.Categories.Any(c => c.Name.Equals(category, StringComparison.OrdinalIgnoreCase)));

        vm.Events = q.ToList();
        return View(vm);
    }

    [HttpGet]
    public IActionResult Error(int? code)
    {
        ViewBag.Code = code ?? 500;
        Response.StatusCode = code ?? 500;
        return View();
    }
}
