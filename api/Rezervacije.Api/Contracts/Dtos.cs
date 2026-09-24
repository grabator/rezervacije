namespace Rezervacije.Api.Contracts;

public record VenueDto(string Slug, string Name, string Subtitle, string? Instagram);

public record FloorElementDto(string Kind, int X, int Y, int W, int H, string? Text);

public record FloorTableDto(string Id, string Label, int X, int Y, int Seats, string Shape, string Status, int Size);

public record FloorPlanDto(int Width, int Height, List<FloorElementDto> Elements, List<FloorTableDto> Tables);

public record VenueEventDto(
    string Id,
    string VenueSlug,
    string Title,
    string Subtitle,
    DateTime StartsAt,
    string? ImageUrl,
    FloorPlanDto FloorPlan
);

public record ReservationRequestDto(string EventId, string TableId, string FullName, string Phone, string Email, string? Note, string? Hp = null);

public record ReservationResultDto(string ReservationId, string Status);

public record ReservationStatusDto(string Id, string Status, string EventTitle, string TableLabel, DateTime CreatedAt);

public record AdminReservationDto(
    string Id,
    string EventId,
    string EventTitle,
    string TableLabel,
    string FullName,
    string Phone,
    string Email,
    string? Note,
    string Status,
    DateTime CreatedAt
);

public record CreateEventDto(string VenueSlug, string Title, string? Subtitle, DateTime StartsAt, string? ImageUrl = null);

public record UpdateEventDto(string Title, string? Subtitle, DateTime StartsAt, string? ImageUrl);
