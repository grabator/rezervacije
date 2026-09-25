using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Rezervacije.Api.Tests;

/// <summary>
/// Javni endpoint /api/reservations/{id}/cancel - gost otkazuje SVOJU rezervaciju preko
/// linka, bez admin lozinke. Klijent ovdje namjerno NEMA X-Admin-Password header, da se
/// dokaze da endpoint stvarno radi bez njega.
/// </summary>
public class GuestCancelTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;

    public GuestCancelTests(TestApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> ReserveTable(string tableId)
    {
        var res = await _client.PostAsJsonAsync("/api/reservations", new
        {
            eventId = "poljska-bih",
            tableId,
            fullName = "Test Gost",
            phone = "+38761234567",
        });
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("reservationId").GetString()!;
    }

    private async Task<string> TableStatus(string tableId)
    {
        var ev = await _client.GetFromJsonAsync<JsonElement>("/api/events/poljska-bih");
        foreach (var t in ev.GetProperty("floorPlan").GetProperty("tables").EnumerateArray())
        {
            if (t.GetProperty("id").GetString() == tableId) return t.GetProperty("status").GetString()!;
        }
        throw new Exception($"Table {tableId} not found");
    }

    [Fact]
    public async Task Guest_CanCancel_PendingReservation_WithoutAdminPassword()
    {
        var id = await ReserveTable("S21");
        Assert.Equal("pending", await TableStatus("S21"));

        var res = await _client.PostAsync($"/api/reservations/{id}/cancel", null);

        res.EnsureSuccessStatusCode();
        Assert.Equal("free", await TableStatus("S21"));

        var status = await _client.GetFromJsonAsync<JsonElement>($"/api/reservations/{id}/status");
        Assert.Equal("cancelled", status.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Guest_CanCancel_ConfirmedReservation_WithoutAdminPassword()
    {
        var id = await ReserveTable("S22");
        var confirmReq = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/reservations/{id}/confirm");
        confirmReq.Headers.Add("X-Admin-Password", TestApiFactory.AdminPassword);
        (await _client.SendAsync(confirmReq)).EnsureSuccessStatusCode();
        Assert.Equal("taken", await TableStatus("S22"));

        var res = await _client.PostAsync($"/api/reservations/{id}/cancel", null);

        res.EnsureSuccessStatusCode();
        Assert.Equal("free", await TableStatus("S22"));
    }

    [Fact]
    public async Task Guest_CannotCancel_AlreadyCancelledReservation()
    {
        var id = await ReserveTable("S23");
        (await _client.PostAsync($"/api/reservations/{id}/cancel", null)).EnsureSuccessStatusCode();

        var res = await _client.PostAsync($"/api/reservations/{id}/cancel", null);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Cancelling_UnknownReservation_Returns404()
    {
        var res = await _client.PostAsync($"/api/reservations/{Guid.NewGuid()}/cancel", null);
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }
}
