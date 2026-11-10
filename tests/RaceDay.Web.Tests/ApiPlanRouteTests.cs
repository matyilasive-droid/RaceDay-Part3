using RaceDay.Web.Services;

namespace RaceDay.Web.Tests;

public class ApiPlanRouteTests
{
    [Fact]
    public void Authentication_and_profile_routes_match_api_plan()
    {
        Assert.Equal("api/auth/register", ApiRoutes.Register);
        Assert.Equal("api/auth/login", ApiRoutes.Login);
        Assert.Equal("api/users/me", ApiRoutes.Profile);
        Assert.Equal("api/users/me/password", ApiRoutes.ChangePassword);
        Assert.Equal("api/user", ApiRoutes.Users);
        Assert.Equal("api/users/user-7", ApiRoutes.User("user-7"));
    }

    [Fact]
    public void Event_and_category_routes_match_api_plan()
    {
        Assert.Equal("api/events", ApiRoutes.Events);
        Assert.Equal("api/events/12", ApiRoutes.Event(12));
        Assert.Equal("api/events/12/categories", ApiRoutes.EventCategories(12));
        Assert.Equal("api/categories/4", ApiRoutes.Category(4));
        Assert.Equal("api/events/12/enrolments", ApiRoutes.EventEnrol(12));
        Assert.Equal("api/events/12/enrolments", ApiRoutes.EventEnrolments(12));
        Assert.Equal("api/event/12/enrolments", ApiRoutes.LegacyEventEnrolments(12));
    }

    [Fact]
    public void Enrolment_and_result_routes_match_api_plan()
    {
        Assert.Equal("api/enrolments", ApiRoutes.Enrolments);
        Assert.Equal("api/enrolments/9", ApiRoutes.Enrolment(9));
        Assert.Equal("api/enrolments/9/results", ApiRoutes.EnrolmentResults(9));
        Assert.Equal("api/enrolments/9/result", ApiRoutes.EnrolmentResult(9));
        Assert.Equal("api/results/3", ApiRoutes.Result(3));
        Assert.Equal("api/events/12/results", ApiRoutes.EventResults(12));
        Assert.Equal("api/users/me/results", ApiRoutes.MyResults);
    }

    [Fact]
    public void Event_type_routes_match_api_plan()
    {
        Assert.Equal("api/event-types", ApiRoutes.EventTypes);
        Assert.Equal("api/event-types/2", ApiRoutes.EventType(2));
    }
}
