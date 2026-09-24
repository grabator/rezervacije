using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Rezervacije.Api.Contracts;
using Rezervacije.Api.Data;
using Rezervacije.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddResponseCompression();
builder.Services.AddHttpClient();

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

// Zastita od spam-rezervacija: limit po IP adresi, podesivo kroz config (testovi ga podignu visoko).
var reservationPermitLimit = builder.Configuration.GetValue<int?>("RateLimiting:ReservationPermitLimit") ?? 5;
var reservationWindowMinutes = builder.Configuration.GetValue<int?>("RateLimiting:ReservationWindowMinutes") ?? 10;

builder.Services.AddRateLimiter(opt =>
{
    opt.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opt.AddPolicy("reservations", ctx => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = reservationPermitLimit,
            Window = TimeSpan.FromMinutes(reservationWindowMinutes),
            QueueLimit = 0,
        }));
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

// Telegram notifikacija adminu o novoj rezervaciji. Opciono - ako nisu podeseni,
// notifikacija se samo tiho preskace (npr. lokalni dev bez Telegram bota).
var telegramBotToken = builder.Configuration["Telegram:BotToken"];
var telegramChatId = builder.Configuration["Telegram:ChatId"];

var app = builder.Build();
var logger = app.Logger;

async Task NotifyAdminViaTelegram(string text, IHttpClientFactory httpFactory)
{
    if (string.IsNullOrWhiteSpace(telegramBotToken) || string.IsNullOrWhiteSpace(telegramChatId)) return;
    try
    {
        var client = httpFactory.CreateClient();
        var url = $"https://api.telegram.org/bot{telegramBotToken}/sendMessage";
        // Bez parse_mode - ime/napomena su slobodan gostov unos i mogli bi sadrzavati
        // karaktere koji bi pokvarili Telegram-ov HTML/Markdown parsing.
        await client.PostAsJsonAsync(url, new { chat_id = telegramChatId, text });
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Slanje Telegram notifikacije adminu nije uspjelo.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(a => a.Run(async ctx =>
    {
        var feature = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        logger.LogError(feature?.Error, "Neuhvacena greska na {Path}", ctx.Request.Path);
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
app.UseRateLimiter();

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

    // Eventi koji su vec prosli (drugi dan i kasnije) se ne prikazuju gostima na listi.
    var today = DateTime.Now.Date;
    var events = await db.Events
        .Where(e => e.VenueId == venue.Id && e.StartsAt >= today)
        .OrderBy(e => e.StartsAt)
        .ToListAsync();
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

app.MapGet("/api/reservations/{id}/status", async (Guid id, AppDbContext db) =>
{
    var r = await db.Reservations.FindAsync(id);
    if (r is null) return Results.NotFound();
    var eventTitle = await db.Events.Where(e => e.Id == r.EventId).Select(e => e.Title).FirstOrDefaultAsync();
    var tableLabel = await db.Tables.Where(t => t.Id == r.TableEntityId).Select(t => t.Label).FirstOrDefaultAsync();
    return Results.Ok(new ReservationStatusDto(r.Id.ToString(), r.Status, eventTitle ?? "", tableLabel ?? "", r.CreatedAt));
});

app.MapPost("/api/reservations", async (ReservationRequestDto req, AppDbContext db, IHttpClientFactory httpFactory) =>
{
    if (!string.IsNullOrWhiteSpace(req.Hp))
    {
        // Honeypot: skriveno polje koje pravi gosti nikad ne popune. Bot koji ga popuni
        // dobija laznu "uspjesnu" potvrdu, bez ikakvog upisa u bazu.
        logger.LogWarning("Honeypot pogodjen na /api/reservations - vjerovatno bot.");
        return Results.Ok(new ReservationResultDto(Guid.NewGuid().ToString(), "pending"));
    }

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
    table.Status = "pending";

    var reservation = new Reservation
    {
        Id = Guid.NewGuid(),
        EventId = req.EventId,
        TableEntityId = table.Id,
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

    logger.LogInformation("Nova rezervacija {Id}: stol {TableId} za event {EventId}", reservation.Id, req.TableId, req.EventId);

    var eventTitle = await db.Events.Where(e => e.Id == req.EventId).Select(e => e.Title).FirstOrDefaultAsync();
    var notifyText = $"🔔 Nova rezervacija\n📅 {eventTitle}\n🪑 Sto {req.TableId}\n👤 {req.FullName}\n📞 {req.Phone}"
        + (string.IsNullOrWhiteSpace(req.Note) ? "" : $"\n💬 {req.Note}");
    await NotifyAdminViaTelegram(notifyText, httpFactory);

    return Results.Ok(new ReservationResultDto(reservation.Id.ToString(), "pending"));
}).RequireRateLimiting("reservations");

// ---- Admin API (zastita lozinkom) ----

// U memoriji drzimo broj pogresnih pokusaja po IP adresi. Nakon 5 pogresnih
// lozinki u kratkom periodu, IP se blokira na 5 minuta - sprecava brute-force
// pogadjanje admin lozinke. Namjerno jednostavno (bez baze/Redis) jer je
// dovoljno za jedan proces na jednom serveru.
var failedAdminAttempts = new ConcurrentDictionary<string, FailedLoginState>();

var admin = app.MapGroup("/api/admin").AddEndpointFilter(async (ctx, next) =>
{
    var ip = ctx.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    var now = DateTime.UtcNow;

    if (failedAdminAttempts.TryGetValue(ip, out var existing) && existing.LockedUntil > now)
    {
        return Results.Json(new { message = "Previše pogrešnih pokušaja. Pokušaj ponovo za par minuta." }, statusCode: StatusCodes.Status429TooManyRequests);
    }

    var provided = ctx.HttpContext.Request.Headers["X-Admin-Password"].ToString();
    if (provided != adminPassword)
    {
        failedAdminAttempts.AddOrUpdate(ip,
            _ => new FailedLoginState(1, now, DateTime.MinValue),
            (_, old) =>
            {
                // Ako je proslo vise od 10 min od zadnjeg pokusaja, brojac krece iznova.
                var count = (now - old.LastAttempt) > TimeSpan.FromMinutes(10) ? 1 : old.Count + 1;
                var lockedUntil = count >= 5 ? now.AddMinutes(5) : DateTime.MinValue;
                return new FailedLoginState(count, now, lockedUntil);
            });
        logger.LogWarning("Neuspjeli admin login pokusaj sa IP {Ip}", ip);
        return Results.Unauthorized();
    }

    failedAdminAttempts.TryRemove(ip, out _);
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
            r.FullName,
            r.Phone,
            r.Email,
            r.Note,
            r.Status,
            r.CreatedAt,
        })
        .ToListAsync();

    var dtos = list.Select(r => new AdminReservationDto(
        r.Id.ToString(), r.EventId, r.EventTitle ?? "", r.TableLabel ?? "",
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
    logger.LogInformation("Rezervacija {Id} potvrdjena", id);
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
    logger.LogInformation("Rezervacija {Id} odbijena", id);
    return Results.Ok();
});

admin.MapPost("/reservations/{id}/cancel", async (Guid id, AppDbContext db) =>
{
    var res = await db.Reservations.FindAsync(id);
    if (res is null) return Results.NotFound();
    if (res.Status != "confirmed")
        return Results.BadRequest(new { message = "Samo potvrđena rezervacija se može otkazati." });

    var table = await db.Tables.FindAsync(res.TableEntityId);
    res.Status = "cancelled";
    if (table is not null) table.Status = "free";
    await db.SaveChangesAsync();
    logger.LogInformation("Rezervacija {Id} otkazana, stol oslobodjen", id);
    return Results.Ok();
});

// ---- Admin: upravljanje eventima ----

admin.MapGet("/events", async (AppDbContext db) =>
{
    var events = await db.Events.Include(e => e.Venue).OrderByDescending(e => e.StartsAt).ToListAsync();
    var dtos = new List<VenueEventDto>();
    foreach (var e in events) dtos.Add(await ToEventDto(e.Id, db, e.Venue!.Slug));
    return Results.Ok(dtos);
});

admin.MapPost("/events", async (CreateEventDto req, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(req.Title) || req.Title.Trim().Length < 3)
        return Results.BadRequest(new { message = "Upiši naziv eventa (min 3 slova)." });
    if (req.StartsAt == default)
        return Results.BadRequest(new { message = "Izaberi datum i vrijeme eventa." });

    var venue = await db.Venues.FirstOrDefaultAsync(v => v.Slug == req.VenueSlug);
    if (venue is null) return Results.BadRequest(new { message = "Nepoznat venue." });

    // Raspored sale je fiksan - kopiramo ga sa najranijeg postojeceg eventa ovog venuea,
    // sa svim stolovima ponovo postavljenim na "slobodno".
    var template = await db.Events.Where(e => e.VenueId == venue.Id).OrderBy(e => e.StartsAt).FirstOrDefaultAsync();
    if (template is null)
        return Results.BadRequest(new { message = "Nema postojećeg rasporeda sale za ovaj venue - kontaktiraj developera." });

    var id = await GenerateUniqueEventId(req.Title, db);

    var ev = new VenueEvent
    {
        Id = id,
        VenueId = venue.Id,
        Title = req.Title.Trim(),
        Subtitle = req.Subtitle?.Trim() ?? "",
        StartsAt = req.StartsAt,
        ImageUrl = string.IsNullOrWhiteSpace(req.ImageUrl) ? null : req.ImageUrl.Trim(),
        FloorWidth = template.FloorWidth,
        FloorHeight = template.FloorHeight,
    };
    db.Events.Add(ev);
    await db.SaveChangesAsync();

    var templateElements = await db.FloorElements.Where(e => e.EventId == template.Id).ToListAsync();
    foreach (var el in templateElements)
        db.FloorElements.Add(new FloorElementEntity { EventId = id, Kind = el.Kind, X = el.X, Y = el.Y, W = el.W, H = el.H, Text = el.Text });

    var templateTables = await db.Tables.Where(t => t.EventId == template.Id).ToListAsync();
    foreach (var t in templateTables)
        db.Tables.Add(new FloorTableEntity { EventId = id, TableKey = t.TableKey, Label = t.Label, X = t.X, Y = t.Y, Size = t.Size, Seats = t.Seats, Shape = t.Shape, Status = "free" });

    await db.SaveChangesAsync();
    logger.LogInformation("Admin kreirao novi event {EventId} ({Title})", id, ev.Title);

    return Results.Ok(await ToEventDto(id, db, venue.Slug));
});

admin.MapPut("/events/{id}", async (string id, UpdateEventDto req, AppDbContext db) =>
{
    var ev = await db.Events.FindAsync(id);
    if (ev is null) return Results.NotFound();

    if (string.IsNullOrWhiteSpace(req.Title) || req.Title.Trim().Length < 3)
        return Results.BadRequest(new { message = "Upiši naziv eventa (min 3 slova)." });
    if (req.StartsAt == default)
        return Results.BadRequest(new { message = "Izaberi datum i vrijeme eventa." });

    ev.Title = req.Title.Trim();
    ev.Subtitle = req.Subtitle?.Trim() ?? "";
    ev.StartsAt = req.StartsAt;
    ev.ImageUrl = string.IsNullOrWhiteSpace(req.ImageUrl) ? null : req.ImageUrl.Trim();
    await db.SaveChangesAsync();
    logger.LogInformation("Admin izmijenio event {EventId}", id);

    var venue = await db.Venues.FindAsync(ev.VenueId);
    return Results.Ok(await ToEventDto(id, db, venue!.Slug));
});

admin.MapDelete("/events/{id}", async (string id, AppDbContext db) =>
{
    var ev = await db.Events.FindAsync(id);
    if (ev is null) return Results.NotFound();

    var hasReservations = await db.Reservations.AnyAsync(r => r.EventId == id);
    if (hasReservations)
        return Results.Conflict(new { message = "Event ima rezervacije i ne može se obrisati." });

    db.FloorElements.RemoveRange(db.FloorElements.Where(e => e.EventId == id));
    db.Tables.RemoveRange(db.Tables.Where(t => t.EventId == id));
    db.Events.Remove(ev);
    await db.SaveChangesAsync();
    logger.LogInformation("Admin obrisao event {EventId}", id);
    return Results.Ok();
});

app.Run();

static async Task<VenueEventDto> ToEventDto(string eventId, AppDbContext db, string venueSlug)
{
    var e = await db.Events.FirstAsync(x => x.Id == eventId);
    var elements = await db.FloorElements.Where(el => el.EventId == eventId)
        .Select(el => new FloorElementDto(el.Kind, el.X, el.Y, el.W, el.H, el.Text))
        .ToListAsync();
    var tables = await db.Tables.Where(t => t.EventId == eventId)
        .Select(t => new FloorTableDto(t.TableKey, t.Label, t.X, t.Y, t.Seats, t.Shape, t.Status, t.Size))
        .ToListAsync();

    return new VenueEventDto(
        e.Id, venueSlug, e.Title, e.Subtitle, e.StartsAt, e.ImageUrl,
        new FloorPlanDto(e.FloorWidth, e.FloorHeight, elements, tables));
}

static async Task<string> GenerateUniqueEventId(string title, AppDbContext db)
{
    var baseSlug = Slugify(title);
    if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "event";

    var slug = baseSlug;
    var n = 2;
    while (await db.Events.AnyAsync(e => e.Id == slug))
    {
        slug = $"{baseSlug}-{n}";
        n++;
    }
    return slug;
}

static string Slugify(string input)
{
    var diacritics = new Dictionary<char, string>
    {
        ['č'] = "c", ['ć'] = "c", ['š'] = "s", ['ž'] = "z", ['đ'] = "dj",
    };

    var sb = new StringBuilder();
    foreach (var ch in input.ToLowerInvariant())
    {
        if (diacritics.TryGetValue(ch, out var replacement)) sb.Append(replacement);
        else if (char.IsLetterOrDigit(ch)) sb.Append(ch);
        else if (ch is ' ' or '-' or '_') sb.Append('-');
    }
    return Regex.Replace(sb.ToString(), "-{2,}", "-").Trim('-');
}

record FailedLoginState(int Count, DateTime LastAttempt, DateTime LockedUntil);

// Omogucava WebApplicationFactory<Program> u test projektu da pokrene ovu app.
public partial class Program { }
