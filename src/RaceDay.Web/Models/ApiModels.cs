using System.Text.Json.Serialization;
namespace RaceDay.Web.Models;

// DTOs shaped like the Part 2 API's JSON responses. Adjust here if your API differs.

public class AuthResponse
{
    public string Token { get; set; } = "";
    [JsonPropertyName("userID")]
    public string UserId { get; set; } = "";
    [JsonPropertyName("name")]
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "Participant";
}

public class CategoryDto
{
    [JsonPropertyName("categoryID")]
    public int Id { get; set; }
    [JsonPropertyName("eventID")]
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public int MaxParticipants { get; set; }
    public decimal EntryFee { get; set; }
}

public class EventDto
{
    [JsonPropertyName("eventID")]
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    [JsonPropertyName("eventDate")]
    public DateTime Date { get; set; }
    [JsonPropertyName("venueID")]
    public int VenueId { get; set; }
    [JsonPropertyName("venue")]
    public string Location { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public string EventType { get; set; } = "";
    public string? BannerUrl { get; set; }
    public int EnrolmentCount { get; set; }
    [JsonPropertyName("organiserID")]
    public string? OrganiserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<CategoryDto> Categories { get; set; } = new();
}

public class EnrolmentDto
{
    [JsonPropertyName("enrolmentID")]
    public int Id { get; set; }
    [JsonPropertyName("eventID")]
    public int EventId { get; set; }
    [JsonPropertyName("eventName")]
    public string EventName { get; set; } = "";
    [JsonPropertyName("eventDate")]
    public DateTime EventDate { get; set; }
    [JsonPropertyName("userID")]
    public string ParticipantId { get; set; } = "";
    public string ParticipantName { get; set; } = "";
    [JsonPropertyName("categoryID")]
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Status { get; set; } = "Pending"; // Pending | Confirmed | Cancelled
    [JsonPropertyName("enrolmentDate")]
    public DateTime EnrolledAt { get; set; }
}

public class ResultDto
{
    [JsonPropertyName("resultID")]
    public int Id { get; set; }
    [JsonPropertyName("enrolmentID")]
    public int EnrolmentId { get; set; }
    [JsonPropertyName("eventID")]
    public int EventId { get; set; }
    [JsonPropertyName("eventName")]
    public string EventName { get; set; } = "";
    [JsonPropertyName("eventDate")]
    public DateTime EventDate { get; set; }
    public string ParticipantName { get; set; } = "";
    public string? CategoryName { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
    public int TotalFinishers { get; set; }
    public bool Published { get; set; }
    public DateTime? RecordedAt { get; set; }
}

public class ProfileDto
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}


/// <summary>Generic response model used by the reusable multipart API client.</summary>
public class UploadResponse
{
    public string Url { get; set; } = "";
}
