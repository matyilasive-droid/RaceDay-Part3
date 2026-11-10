using System;
using System.Collections.Generic;

namespace RaceDay.Api.Models;

public partial class Event
{
    public int EventId { get; set; }

    public int Organised { get; set; }

    public string EventName { get; set; } = null!;

    public string EventType { get; set; } = null!;

    public DateOnly EventDate { get; set; }

    public string Location { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string Province { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual EventType EventTypeNavigation { get; set; } = null!;

    public virtual User OrganisedNavigation { get; set; } = null!;
}
