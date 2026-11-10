namespace RaceDay.Web.Services;

/// <summary>Single place that lists every Part 2 API endpoint the MVC app calls.</summary>
public static class ApiRoutes
{
    public const string Login = "api/auth/login";
    public const string Register = "api/auth/register";
    public const string Events = "api/events";
    public const string MyEvents = "api/events/mine";
    public const string MyEnrolments = "api/enrolments/mine";
    public const string MyResults = "api/results/mine";
    public const string Profile = "api/profile";
    public const string ProfilePicture = "api/profile/picture";

    public static string Event(int id) => $"api/events/{id}";
    public static string EventBanner(int id) => $"api/events/{id}/banner";
    public static string EventCategories(int id) => $"api/events/{id}/categories";
    public static string EventCategory(int id, int categoryId) => $"api/events/{id}/categories/{categoryId}";
    public static string EventEnrol(int id) => $"api/events/{id}/enrol";
    public static string EventEnrolments(int id) => $"api/events/{id}/enrolments";
    public static string EventResults(int id) => $"api/events/{id}/results";
    public static string EventResultsPublish(int id) => $"api/events/{id}/results/publish";
}
