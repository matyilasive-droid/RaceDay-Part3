using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

public class LoginVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}

public class RegisterVm
{
    [Required, Display(Name = "First name"), StringLength(50)]
    public string FirstName { get; set; } = "";

    [Required, Display(Name = "Last name"), StringLength(50)]
    public string LastName { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters."), DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required, DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Passwords do not match."), Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = "";

}

public class EventFormVm
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = "";

    [Required, StringLength(4000)]
    public string Description { get; set; } = "";

    [Required, DataType(DataType.DateTime)]
    public DateTime Date { get; set; } = DateTime.Today.AddDays(30).AddHours(9);

    [Required, Range(1, int.MaxValue, ErrorMessage = "Select a valid venue.")]
    [Display(Name = "Venue ID")]
    public int VenueId { get; set; }

    [Required, Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km."), Display(Name = "Distance (km)")]
    public decimal DistanceKm { get; set; } = 5;

    [Required, Display(Name = "Event type")]
    public string EventType { get; set; } = "Road Run";

    public static readonly string[] EventTypes = { "Road Run", "Trail Run", "Cycling", "Triathlon", "Swim", "Walk", "Other" };
}

public class CategoryFormVm
{
    [Required, StringLength(60)]
    public string Name { get; set; } = "";

    [Range(0.1, 500, ErrorMessage = "Distance must be greater than zero.")]
    public decimal DistanceKm { get; set; } = 5;

    [Range(1, 100000)]
    public int MaxParticipants { get; set; } = 100;

    [Range(0, 100000)]
    public decimal EntryFee { get; set; } = 0;
}

public class ProfileVm
{
    [Required, Display(Name = "First name"), StringLength(50)]
    public string FirstName { get; set; } = "";

    [Required, Display(Name = "Last name"), StringLength(50)]
    public string LastName { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

}

public class HomeVm
{
    public List<EventDto> Events { get; set; } = new();
    public string? Search { get; set; }
    public string? Type { get; set; }
    public string? Category { get; set; }
    public List<string> Types { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public string? Error { get; set; }
}

public class EventDetailsVm
{
    public EventDto Event { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
}

/// <summary>Shared by the organiser categories / enrolments / results pages.</summary>
public class EventPageVm
{
    public EventDto Event { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public List<EnrolmentDto> Enrolments { get; set; } = new();
    public List<ResultDto> Results { get; set; } = new();
    public CategoryFormVm NewCategory { get; set; } = new();
}
