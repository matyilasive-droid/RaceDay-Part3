namespace RaceDay.Web.Helpers;

public static class Format
{
    public static string Ordinal(int n)
    {
        var lastTwo = n % 100;
        if (lastTwo is >= 11 and <= 13) return $"{n}th";
        return (n % 10) switch { 1 => $"{n}st", 2 => $"{n}nd", 3 => $"{n}rd", _ => $"{n}th" };
    }

    public static string Time(TimeSpan t) => $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";

    public static string StatusClass(string? status) => (status ?? "").ToLowerInvariant() switch
    {
        "confirmed" => "status-confirmed",
        "pending" => "status-pending",
        "cancelled" or "canceled" => "status-cancelled",
        "waitlisted" => "status-waitlist",
        _ => "status-other"
    };

    public static bool IsLocalUrl(string? url) =>
        !string.IsNullOrEmpty(url) && url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'));
}

public static class UploadValidator
{
    public const long MaxBytes = 5 * 1024 * 1024;
    private static readonly string[] Allowed = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    /// <summary>Returns an error message, or null when the file is acceptable.</summary>
    public static string? Validate(IFormFile? file)
    {
        if (file is null || file.Length == 0) return "Please choose an image to upload.";
        if (file.Length > MaxBytes) return "Image must be 5 MB or smaller.";
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(ext)) return "Only JPG, PNG, WEBP or GIF images are allowed.";
        return null;
    }
}
