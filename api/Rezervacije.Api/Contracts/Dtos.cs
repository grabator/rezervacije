namespace Rezervacije.Api.Contracts;

public record VenueDto(string Slug, string Name, string Subtitle, string? Instagram);

public record TablePackageDto(string Id, string Name, int Persons, decimal Price, string Description);

public record FloorElementDto(string Kind, int X, int Y, int W, int H, string? Text);

public record FloorTableDto(string Id, string Label, int X, int Y, int Seats, string Shape, string Status);

public record FloorPlanDto(int Width, int Height, List<FloorElementDto> Elements, List<FloorTableDto> Tables);

public record VenueEventDto(
    string Id,
    string VenueSlug,
    string Title,
    string Subtitle,
    DateTime StartsAt,
    string? ImageUrl,
    List<TablePackageDto> Packages,
    FloorPlanDto FloorPlan
);

public record ReservationRequestDto(string EventId, string TableId, string? PackageId, string FullName, string Phone, string Email, string? Note);

public record ReservationResultDto(string ReservationId, string Status);

public record AdminReservationDto(
    string Id,
    string EventId,
    string EventTitle,
    string TableLabel,
    string? PackageName,
    string FullName,
    string Phone,
    string Email,
    string? Note,
    string Status,
    DateTime CreatedAt
);
