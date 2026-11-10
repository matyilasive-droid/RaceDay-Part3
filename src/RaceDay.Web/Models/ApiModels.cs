namespace RaceDay.Web.Models;

// DTOs shaped like the Part 2 API's JSON responses. Adjust here if your API differs.

public class AuthResponse
{
    public string Token { get; set; } = "";
    public string UserId { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "Participant";
    public string? ProfileImageUrl { get; set; }
}

public class CategoryDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "Age"; // Age | Distance
}

public class EventDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime Date { get; set; }
    public string Location { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public string EventType { get; set; } = "";
    public string? BannerUrl { get; set; }
    public int EnrolmentCount { get; set; }
    public string? OrganiserId { get; set; }
    public List<CategoryDto> Categories { get; set; } = new();
}

public class EnrolmentDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string ParticipantId { get; set; } = "";
    public string ParticipantName { get; set; } = "";
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Status { get; set; } = "Pending"; // Pending | Confirmed | Cancelled
    public DateTime EnrolledAt { get; set; }
}

public class ResultDto
{
    public int Id { get; set; }
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string ParticipantName { get; set; } = "";
    public string? CategoryName { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
    public int TotalFinishers { get; set; }
    public bool Published { get; set; }
}

public class ProfileDto
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? ProfileImageUrl { get; set; }
}

public class UploadResponse
{
    public string Url { get; set; } = "";
}
