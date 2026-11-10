using System.Text.Json;

namespace RaceDay.Web.Services;

public record SessionUser(string Token, string UserId, string FullName, string Email, string Role, string? ProfileImageUrl)
{
    public bool IsOrganiser => Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase);
}

public static class RaceDaySessionExtensions
{
    private const string Key = "raceday.user";

    public static void SetUser(this ISession session, SessionUser user) =>
        session.SetString(Key, JsonSerializer.Serialize(user));

    public static SessionUser? GetUser(this ISession session)
    {
        var json = session.GetString(Key);
        return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<SessionUser>(json);
    }
}
