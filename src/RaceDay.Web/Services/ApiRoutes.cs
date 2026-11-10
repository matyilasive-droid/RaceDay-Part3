namespace RaceDay.Web.Services;

/// <summary>
/// Routes defined by API PLAN.pdf (Section B). Keep route spelling and HTTP
/// resource boundaries here so the MVC client does not invent API endpoints.
/// </summary>
public static class ApiRoutes
{
    public const string Register = "api/auth/register";
    public const string Login = "api/auth/login";
    public const string Profile = "api/users/me";
    public const string Users = "api/user";
    public const string MyResults = "api/users/me/results";
    public const string Events = "api/events";
    public const string Enrolments = "api/enrolments";
    public const string MyEnrolments = Enrolments;
    public const string EventTypes = "api/event-types";

    public static string User(string userId) => $"api/users/{Uri.EscapeDataString(userId)}";
    public static string ChangePassword => "api/users/me/password";
    public static string Event(int eventId) => $"api/events/{eventId}";
    public static string EventEnrolments(int eventId) => $"api/events/{eventId}/enrolments";
    public static string LegacyEventEnrolments(int eventId) => $"api/event/{eventId}/enrolments";
    public static string EventCategories(int eventId) => $"api/events/{eventId}/categories";
    public static string Category(int categoryId) => $"api/categories/{categoryId}";
    public static string EventCategory(int eventId, int categoryId) => Category(categoryId);
    public static string EventEnrol(int eventId) => $"api/events/{eventId}/enrolments";
    public static string Enrolment(int enrolmentId) => $"api/enrolments/{enrolmentId}";
    public static string EnrolmentResult(int enrolmentId) => $"api/enrolments/{enrolmentId}/result";
    public static string EnrolmentResults(int enrolmentId) => $"api/enrolments/{enrolmentId}/results";
    public static string EventResults(int eventId) => $"api/events/{eventId}/results";
    public static string Result(int resultId) => $"api/results/{resultId}";
    public static string EventType(int eventTypeId) => $"api/event-types/{eventTypeId}";
}
