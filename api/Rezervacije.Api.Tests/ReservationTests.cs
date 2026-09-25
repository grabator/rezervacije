using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Rezervacije.Api.Tests;

public class ReservationTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;

    public ReservationTests(TestApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static object ValidRequest(string tableId, string? fullName = null, string? phone = null) => new
    {
        eventId = "poljska-bih",
        tableId,
        fullName = fullName ?? "Test Gost",
        phone = phone ?? "+38761234567",
    };

    [Fact]
    public async Task Reserving_FreeTable_Succeeds()
    {
        var res = await _client.PostAsJsonAsync("/api/reservations", ValidRequest("S1"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("pending", body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Reserving_SameTableTwice_SecondRequestIsRejected()
    {
        var first = await _client.PostAsJsonAsync("/api/reservations", ValidRequest("S2"));
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        // Ovo je NAJVAZNIJI test u cijelom projektu - ako ovo ikad prestane
        // da vraca 409, dvoje ljudi moze rezervisati isti sto.
        var second = await _client.PostAsJsonAsync("/api/reservations", ValidRequest("S2"));
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Reserving_UnknownTable_ReturnsConflict()
    {
        var res = await _client.PostAsJsonAsync("/api/reservations", ValidRequest("ne-postoji-123"));
        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
    }

    [Theory]
    [InlineData("", "+38761234567")]     // prazno ime
    [InlineData("ab", "+38761234567")]   // prekratko ime
    [InlineData("Test Gost", "123")]     // prekratak telefon
    public async Task Reserving_WithInvalidInput_ReturnsBadRequest(string name, string phone)
    {
        var res = await _client.PostAsJsonAsync("/api/reservations", ValidRequest("S5", name, phone));
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Reserving_WithHoneypotFilled_SilentlySucceedsWithoutBookingTable()
    {
        var req = new
        {
            eventId = "poljska-bih",
            tableId = "S15",
            fullName = "Bot Test",
            phone = "+38761234567",
            hp = "im-a-bot",
        };

        var res = await _client.PostAsJsonAsync("/api/reservations", req);
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var ev = await _client.GetFromJsonAsync<JsonElement>("/api/events/poljska-bih");
        var status = ev.GetProperty("floorPlan").GetProperty("tables").EnumerateArray()
            .First(t => t.GetProperty("id").GetString() == "S15").GetProperty("status").GetString();
        Assert.Equal("free", status);
    }

    [Fact]
    public async Task ReservationStatus_ForExistingReservation_ReturnsStatusAndTable()
    {
        var createRes = await _client.PostAsJsonAsync("/api/reservations", ValidRequest("S16"));
        var body = await createRes.Content.ReadFromJsonAsync<JsonElement>();
        var id = body.GetProperty("reservationId").GetString();

        var statusRes = await _client.GetAsync($"/api/reservations/{id}/status");

        Assert.Equal(HttpStatusCode.OK, statusRes.StatusCode);
        var statusBody = await statusRes.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("pending", statusBody.GetProperty("status").GetString());
        Assert.Equal("S16", statusBody.GetProperty("tableLabel").GetString());
    }

    [Fact]
    public async Task ReservationStatus_ForUnknownId_Returns404()
    {
        var res = await _client.GetAsync($"/api/reservations/{Guid.NewGuid()}/status");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }
}
