using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RaceDay.Web.Controllers;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Tests;

public class ControllerTests
{
    private class NullTempData : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(Microsoft.AspNetCore.Http.HttpContext c) => new Dictionary<string, object>();
        public void SaveTempData(Microsoft.AspNetCore.Http.HttpContext c, IDictionary<string, object> v) { }
    }

    private static T Setup<T>(T controller, SessionUser? user = null) where T : Controller
    {
        var ctx = Ctx.WithSession(user);
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };
        controller.TempData = new TempDataDictionary(ctx, new NullTempData());
        return controller;
    }

    private static EventDto Ev(int id, string name, string type, int days, params string[] cats) => new()
    {
        Id = id, Name = name, EventType = type, Location = "Cardiff", Date = DateTime.Today.AddDays(days),
        Categories = cats.Select((c, i) => new CategoryDto { Id = i + 1, Name = c }).ToList()
    };

    [Fact]
    public async Task Home_hides_past_events_and_sorts_by_date()
    {
        var api = new FakeApiClient();
        api.Responses[ApiRoutes.Events] = new List<EventDto> { Ev(1, "Past", "Road Run", -5), Ev(2, "Later", "Road Run", 20), Ev(3, "Soon", "Road Run", 3) };
        var vm = (HomeVm)((ViewResult)await Setup(new HomeController(api)).Index(null, null, null)).Model!;
        Assert.Equal(new[] { "Soon", "Later" }, vm.Events.Select(e => e.Name));
    }

    [Fact]
    public async Task Home_search_filters_by_name_and_location()
    {
        var api = new FakeApiClient();
        api.Responses[ApiRoutes.Events] = new List<EventDto> { Ev(1, "Bay 10K", "Road Run", 5), Ev(2, "Hill Trail", "Trail Run", 6) };
        var vm = (HomeVm)((ViewResult)await Setup(new HomeController(api)).Index("hill", null, null)).Model!;
        Assert.Single(vm.Events);
        Assert.Equal("Hill Trail", vm.Events[0].Name);
    }

    [Fact]
    public async Task Home_filters_by_type_and_category()
    {
        var api = new FakeApiClient();
        api.Responses[ApiRoutes.Events] = new List<EventDto> { Ev(1, "A", "Road Run", 5, "Under 18"), Ev(2, "B", "Trail Run", 6, "Masters") };
        var c = Setup(new HomeController(api));
        var byType = (HomeVm)((ViewResult)await c.Index(null, "Trail Run", null)).Model!;
        var byCat = (HomeVm)((ViewResult)await c.Index(null, null, "Under 18")).Model!;
        Assert.Equal("B", Assert.Single(byType.Events).Name);
        Assert.Equal("A", Assert.Single(byCat.Events).Name);
    }

    [Fact]
    public async Task Home_shows_error_but_does_not_crash_when_api_down()
    {
        var api = new FakeApiClient();
        api.Errors[ApiRoutes.Events] = (503, "Cannot reach the RaceDay service.");
        var vm = (HomeVm)((ViewResult)await Setup(new HomeController(api)).Index(null, null, null)).Model!;
        Assert.Empty(vm.Events);
        Assert.NotNull(vm.Error);
    }

    [Fact]
    public async Task Enter_success_redirects_to_my_enrolments()
    {
        var api = new FakeApiClient();
        api.Responses[ApiRoutes.EventEnrol(5)] = new EnrolmentDto { Id = 1 };
        var result = await Setup(new EventsController(api), Ctx.Participant()).Enter(5, 2);
        Assert.Equal("MyEnrolments", Assert.IsType<RedirectToActionResult>(result).ControllerName);
    }

    [Fact]
    public async Task Enter_failure_returns_to_details_with_error()
    {
        var api = new FakeApiClient();
        api.Errors[ApiRoutes.EventEnrol(5)] = (409, "Already entered");
        var c = Setup(new EventsController(api), Ctx.Participant());
        var result = await c.Enter(5, 2);
        Assert.Equal("Details", Assert.IsType<RedirectToActionResult>(result).ActionName);
        Assert.Equal("Already entered", c.TempData["Error"]);
    }

    [Fact]
    public async Task Organiser_dashboard_loads_own_events()
    {
        var api = new FakeApiClient();
        var own = Ev(1, "A", "Road Run", 9); own.OrganiserId = "u2";
        var other = Ev(2, "B", "Road Run", 2); other.OrganiserId = "other";
        api.Responses[ApiRoutes.Events] = new List<EventDto> { own, other };
        var model = (List<EventDto>)((ViewResult)await Setup(new OrganiserController(api), Ctx.Organiser()).Dashboard()).Model!;
        Assert.Equal("A", model[0].Name);
        Assert.Single(model);
        Assert.Contains("GET " + ApiRoutes.Events, api.Calls);
    }

    [Fact]
    public async Task SaveResult_rejects_bad_time_without_calling_api()
    {
        var api = new FakeApiClient();
        var c = Setup(new OrganiserController(api), Ctx.Organiser());
        await c.SaveResult(1, 9, "abc", 3);
        Assert.Empty(api.Calls);
        Assert.NotNull(c.TempData["Error"]);
    }

    [Fact]
    public async Task SaveResult_rejects_bad_position()
    {
        var api = new FakeApiClient();
        var c = Setup(new OrganiserController(api), Ctx.Organiser());
        await c.SaveResult(1, 9, "00:45:10", 0);
        Assert.Empty(api.Calls);
    }

    [Fact]
    public async Task SaveResult_posts_valid_result()
    {
        var api = new FakeApiClient();
        api.Responses[ApiRoutes.EnrolmentResults(9)] = new ResultDto();
        var c = Setup(new OrganiserController(api), Ctx.Organiser());
        await c.SaveResult(1, 9, "00:45:10", 3);
        Assert.Contains("POST " + ApiRoutes.EnrolmentResults(9), api.Calls);
        Assert.Equal("Result saved.", c.TempData["Success"]);
    }

    [Fact]
    public async Task DeleteEvent_calls_api_delete()
    {
        var api = new FakeApiClient();
        await Setup(new OrganiserController(api), Ctx.Organiser()).DeleteEvent(4);
        Assert.Contains("DELETE " + ApiRoutes.Event(4), api.Calls);
    }

    [Fact]
    public async Task MyResults_orders_newest_first()
    {
        var api = new FakeApiClient();
        api.Responses[ApiRoutes.MyResults] = new List<ResultDto>
        {
            new() { EventName = "Old", EventDate = DateTime.Today.AddDays(-90) },
            new() { EventName = "New", EventDate = DateTime.Today.AddDays(-5) }
        };
        var model = (List<ResultDto>)((ViewResult)await Setup(new MyResultsController(api), Ctx.Participant()).Index()).Model!;
        Assert.Equal("New", model[0].EventName);
    }

}
