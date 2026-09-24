using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Rezervacije.Api.Tests;

public class AdminEventsTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;

    public AdminEventsTests(TestApiFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Admin-Password", TestApiFactory.AdminPassword);
    }

    [Fact]
    public async Task CreatingEvent_CopiesFloorPlanWithAllTablesFree()
    {
        var res = await _client.PostAsJsonAsync("/api/admin/events", new
        {
            venueSlug = "exclusive",
            title = "Test Turnir",
            subtitle = "Probni event",
            startsAt = DateTime.Now.AddDays(10),
        });

        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        var id = body.GetProperty("id").GetString()!;
        Assert.StartsWith("test-turnir", id);

        var tables = body.GetProperty("floorPlan").GetProperty("tables").EnumerateArray().ToList();
        Assert.NotEmpty(tables);
        Assert.All(tables, t => Assert.Equal("free", t.GetProperty("status").GetString()));
    }

    [Fact]
    public async Task CreatingEvent_WithShortTitle_ReturnsBadRequest()
    {
        var res = await _client.PostAsJsonAsync("/api/admin/events", new
        {
            venueSlug = "exclusive",
            title = "ab",
            startsAt = DateTime.Now.AddDays(10),
        });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task DeletingEvent_WithoutReservations_Succeeds()
    {
        var create = await _client.PostAsJsonAsync("/api/admin/events", new
        {
            venueSlug = "exclusive",
            title = "Za Brisanje",
            startsAt = DateTime.Now.AddDays(11),
        });
        var body = await create.Content.ReadFromJsonAsync<JsonElement>();
        var id = body.GetProperty("id").GetString()!;

        var res = await _client.DeleteAsync($"/api/admin/events/{id}");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task DeletingEvent_WithReservations_ReturnsConflict()
    {
        await _client.PostAsJsonAsync("/api/reservations", new
        {
            eventId = "poljska-bih",
            tableId = "S20",
            fullName = "Test Gost",
            phone = "+38761234567",
            email = "gost@test.com",
        });

        var res = await _client.DeleteAsync("/api/admin/events/poljska-bih");

        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
    }

    [Fact]
    public async Task UpdatingEvent_ChangesTitleDateAndImage_WithoutTouchingTables()
    {
        var create = await _client.PostAsJsonAsync("/api/admin/events", new
        {
            venueSlug = "exclusive",
            title = "Za Izmjenu",
            startsAt = DateTime.Now.AddDays(12),
        });
        var createdBody = await create.Content.ReadFromJsonAsync<JsonElement>();
        var id = createdBody.GetProperty("id").GetString()!;
        var newDate = DateTime.Now.AddDays(20);

        var res = await _client.PutAsJsonAsync($"/api/admin/events/{id}", new
        {
            title = "Ispravljen Naziv",
            subtitle = "Novi podnaslov",
            startsAt = newDate,
            imageUrl = "https://example.com/slika.jpg",
        });

        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Ispravljen Naziv", body.GetProperty("title").GetString());
        Assert.Equal("Novi podnaslov", body.GetProperty("subtitle").GetString());
        Assert.Equal("https://example.com/slika.jpg", body.GetProperty("imageUrl").GetString());
        Assert.Equal(27, body.GetProperty("floorPlan").GetProperty("tables").GetArrayLength());
    }

    [Fact]
    public async Task UpdatingUnknownEvent_Returns404()
    {
        var res = await _client.PutAsJsonAsync("/api/admin/events/ne-postoji", new
        {
            title = "Nešto",
            startsAt = DateTime.Now.AddDays(1),
        });

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }
}
