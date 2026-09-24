using Microsoft.EntityFrameworkCore;
using Rezervacije.Api.Contracts;
using Rezervacije.Api.Data;
using Rezervacije.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddResponseCompression();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=rezervacije.db"));

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var adminPassword = builder.Configuration["AdminPassword"];
if (builder.Environment.IsProduction() && string.IsNullOrWhiteSpace(adminPassword))
{
    // Fail-fast: bolje da se app uopste ne pokrene u produkciji nego da radi
    // sa praznom/pogadjivom admin lozinkom. Postavi AdminPassword kao pravi
    // secret (env var / hosting platform secret), ne u appsettings.json.
    throw new InvalidOperationException(
        "AdminPassword nije postavljen. U produkciji mora biti pravi secret " +
        "(environment varijabla), ne vrijednost iz appsettings.json.");
}
adminPassword ??= "promijeni-me";

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(a => a.Run(async ctx =>
    {
        ctx.Response.StatusCode = 500;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsJsonAsync(new { message = "Došlo je do greške na serveru." });
    }));
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.EnsureSeeded(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseResponseCompression();
app.UseCors();

// ---- Javni API (frontend za goste) ----

app.MapGet("/api/venues/{slug}", async (string slug, AppDbContext db) =>
{
    var v = await db.Venues.FirstOrDefaultAsync(x => x.Slug == slug);
    return v is null ? Results.NotFound() : Results.Ok(new VenueDto(v.Slug, v.Name, v.Subtitle, v.Instagram));
});

app.MapGet("/api/venues/{slug}/events", async (string slug, AppDbContext db) =>
{
    var venue = await db.Venues.FirstOrDefaultAsync(x => x.Slug == slug);
    if (venue is null) return Results.NotFound();

    var events = await db.Events.Where(e => e.VenueId == venue.Id).ToListAsync();
    var dtos = new List<VenueEventDto>();
    foreach (var e in events) dtos.Add(await ToEventDto(e.Id, db, venue.Slug));
    return Results.Ok(dtos);
});

app.MapGet("/api/events/{id}", async (string id, AppDbContext db) =>
{
    var e = await db.Events.Include(x => x.Venue).FirstOrDefaultAsync(x => x.Id == id);
    if (e is null) return Results.NotFound();
    return Results.Ok(await ToEventDto(id, db, e.Venue!.Slug));
});

app.MapPost("/api/reservations", async (ReservationRequestDto req, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(req.FullName) || req.FullName.Trim().Length < 3)
        return Results.BadRequest(new { message = "Upiši ime i prezime." });
    if (string.IsNullOrWhiteSpace(req.Phone) || req.Phone.Trim().Length < 6)
        return Results.BadRequest(new { message = "Upiši ispravan broj telefona." });
    if (string.IsNullOrWhiteSpace(req.Email) || !req.Email.Contains('@') || !req.Email.Contains('.'))
        return Results.BadRequest(new { message = "Upiši ispravan email." });
    if (string.IsNullOrWhiteSpace(req.EventId) || string.IsNullOrWhiteSpace(req.TableId))
        return Results.BadRequest(new { message = "Nedostaju podaci o eventu ili stolu." });

    using var tx = await db.Database.BeginTransactionAsync();

    var table = await db.Tables.FirstOrDefaultAsync(t => t.EventId == req.EventId && t.TableKey == req.TableId);
    if (table is null || table.Status != "free")
    {
        return Results.Conflict(new { message = "Ovaj stol je u međuvremenu zauzet. Izaberi drugi." });
    }
    string? packageId = null;
    if (!string.IsNullOrWhiteSpace(req.PackageId))
    {
        var package = await db.Packages.FirstOrDefaultAsync(p => p.Id == req.PackageId && p.EventId == req.EventId);
        if (package is null)
        {
            return Results.BadRequest(new { message = "Nepoznat paket." });
        }
        packageId = package.Id;
    }

    table.Status = "pending";

    var reservation = new Reservation
    {
        Id = Guid.NewGuid(),
        EventId = req.EventId,
        TableEntityId = table.Id,
        PackageId = packageId,
        FullName = req.FullName,
        Phone = req.Phone,
        Email = req.Email,
        Note = req.Note,
        Status = "pending",
        CreatedAt = DateTime.UtcNow,
    };
    db.Reservations.Add(reservation);
    await db.SaveChangesAsync();
    await tx.CommitAsync();

    return Results.Ok(new ReservationResultDto(reservation.Id.ToString(), "pending"));
});

// ---- Admin API (zastita lozinkom) ----

var admin = app.MapGroup("/api/admin").AddEndpointFilter(async (ctx, next) =>
{
    var provided = ctx.HttpContext.Request.Headers["X-Admin-Password"].ToString();
    if (provided != adminPassword)
    {
        return Results.Unauthorized();
    }
    return await next(ctx);
});

admin.MapGet("/reservations", async (AppDbContext db) =>
{
    var list = await db.Reservations
        .OrderByDescending(r => r.CreatedAt)
        .Select(r => new
        {
            r.Id,
            r.EventId,
            EventTitle = db.Events.Where(e => e.Id == r.EventId).Select(e => e.Title).FirstOrDefault(),
            TableLabel = db.Tables.Where(t => t.Id == r.TableEntityId).Select(t => t.Label).FirstOrDefault(),
            PackageName = db.Packages.Where(p => p.Id == r.PackageId).Select(p => p.Name).FirstOrDefault(),
            r.FullName,
            r.Phone,
            r.Email,
            r.Note,
            r.Status,
            r.CreatedAt,
        })
        .ToListAsync();

    var dtos = list.Select(r => new AdminReservationDto(
        r.Id.ToString(), r.EventId, r.EventTitle ?? "", r.TableLabel ?? "", r.PackageName ?? "",
        r.FullName, r.Phone, r.Email, r.Note, r.Status, r.CreatedAt));
    return Results.Ok(dtos);
});

admin.MapPost("/reservations/{id}/confirm", async (Guid id, AppDbContext db) =>
{
    var res = await db.Reservations.FindAsync(id);
    if (res is null) return Results.NotFound();
    var table = await db.Tables.FindAsync(res.TableEntityId);
    res.Status = "confirmed";
    if (table is not null) table.Status = "taken";
    await db.SaveChangesAsync();
    return Results.Ok();
});

admin.MapPost("/reservations/{id}/reject", async (Guid id, AppDbContext db) =>
{
    var res = await db.Reservations.FindAsync(id);
    if (res is null) return Results.NotFound();
    var table = await db.Tables.FindAsync(res.TableEntityId);
    res.Status = "rejected";
    if (table is not null) table.Status = "free";
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run();

static async Task<VenueEventDto> ToEventDto(string eventId, AppDbContext db, string venueSlug)
{
    var e = await db.Events.FirstAsync(x => x.Id == eventId);
    var packages = await db.Packages.Where(p => p.EventId == eventId)
        .Select(p => new TablePackageDto(p.Id, p.Name, p.Persons, p.Price, p.Description))
        .ToListAsync();
    var elements = await db.FloorElements.Where(el => el.EventId == eventId)
        .Select(el => new FloorElementDto(el.Kind, el.X, el.Y, el.W, el.H, el.Text))
        .ToListAsync();
    var tables = await db.Tables.Where(t => t.EventId == eventId)
        .Select(t => new FloorTableDto(t.TableKey, t.Label, t.X, t.Y, t.Seats, t.Shape, t.Status, t.Size))
        .ToListAsync();

    return new VenueEventDto(
        e.Id, venueSlug, e.Title, e.Subtitle, e.StartsAt, e.ImageUrl,
        packages, new FloorPlanDto(e.FloorWidth, e.FloorHeight, elements, tables));
}

// Omogucava WebApplicationFactory<Program> u test projektu da pokrene ovu app.
public partial class Program { }
