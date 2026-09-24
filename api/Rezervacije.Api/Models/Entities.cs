namespace Rezervacije.Api.Models;

public class Venue
{
    public int Id { get; set; }
    public string Slug { get; set; } = "";
    public string Name { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string? Instagram { get; set; }

    public List<VenueEvent> Events { get; set; } = [];
}

public class VenueEvent
{
    public string Id { get; set; } = "";
    public int VenueId { get; set; }
    public Venue? Venue { get; set; }
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public DateTime StartsAt { get; set; }
    public string? ImageUrl { get; set; }
    public int FloorWidth { get; set; }
    public int FloorHeight { get; set; }

    public List<FloorElementEntity> Elements { get; set; } = [];
    public List<FloorTableEntity> Tables { get; set; } = [];
}

/// <summary>Statični dijelovi sale: šank, TV, sofa, ulaz, natpis.</summary>
public class FloorElementEntity
{
    public int Id { get; set; }
    public string EventId { get; set; } = "";
    public string Kind { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public int W { get; set; }
    public int H { get; set; }
    public string? Text { get; set; }
}

public class FloorTableEntity
{
    public int Id { get; set; }
    public string EventId { get; set; } = "";
    public string TableKey { get; set; } = "";
    public string Label { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public int Size { get; set; } = 48;
    public int Seats { get; set; }
    public string Shape { get; set; } = "round";
    public string Status { get; set; } = "free";
}

public class Reservation
{
    public Guid Id { get; set; }
    public string EventId { get; set; } = "";
    public VenueEvent? Event { get; set; }
    public int TableEntityId { get; set; }
    public FloorTableEntity? Table { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Note { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
}
