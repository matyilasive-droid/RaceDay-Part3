namespace RaceDay.Api.DTOs;

public class UserDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UserCreateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    // Plain password provided by the client; will be hashed on the server.
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Runner";
}

public class EventDto
{
    public int EventId { get; set; }
    public int Organised { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateOnly EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class EventCreateDto
{
    public int Organised { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateOnly EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string Status { get; set; } = "Upcoming";
}

public class CategoryDto
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public int? MaxParticipant { get; set; }
    public decimal EntryFee { get; set; }
}

public class CategoryCreateDto
{
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public int? MaxParticipant { get; set; }
    public decimal EntryFee { get; set; }
}

public class EnrolmentDto
{
    public int EnrolmentId { get; set; }
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class EnrolmentCreateDto
{
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public string Status { get; set; } = "Registered";
}

public class ResultDto
{
    public int ResultId { get; set; }
    public int Enrolment { get; set; }
    public TimeOnly FinishTime { get; set; }
    public int Position { get; set; }
    public DateTime RecordedAt { get; set; }
}

public class ResultCreateDto
{
    public int Enrolment { get; set; }
    public TimeOnly FinishTime { get; set; }
    public int Position { get; set; }
}
