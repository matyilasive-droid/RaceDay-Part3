using System;
using System.Collections.Generic;

namespace RaceDay.Api.Models;

public partial class VwEventCategory
{
    public int EventId { get; set; }

    public string EventName { get; set; } = null!;

    public DateOnly EventDate { get; set; }

    public string Location { get; set; } = null!;

    public string Province { get; set; } = null!;

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public decimal DistanceKm { get; set; }

    public decimal EntryFee { get; set; }
}
