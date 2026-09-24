using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Rezervacije.Api.Tests;

public class ConfirmRejectTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;

    public ConfirmRejectTests(TestApiFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Admin-Password", TestApiFactory.AdminPassword);
    }

    private async Task<string> ReserveTable(string tableId)
    {
        var res = await _client.PostAsJsonAsync("/api/reservations", new
        {
            eventId = "poljska-bih",
            tableId,
            fullName = "Test Gost",
            phone = "+38761234567",
            email = "gost@test.com",
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
    public async Task Confirming_Reservation_MarksTableAsTaken()
    {
        var id = await ReserveTable("S8");

        var res = await _client.PostAsync($"/api/admin/reservations/{id}/confirm", null);

        res.EnsureSuccessStatusCode();
        Assert.Equal("taken", await TableStatus("S8"));
    }

    [Fact]
    public async Task Rejecting_Reservation_FreesTheTableAgain()
    {
        var id = await ReserveTable("S10");
        Assert.Equal("pending", await TableStatus("S10"));

        var res = await _client.PostAsync($"/api/admin/reservations/{id}/reject", null);

        res.EnsureSuccessStatusCode();
        Assert.Equal("free", await TableStatus("S10"));
    }

    [Fact]
    public async Task Cancelling_ConfirmedReservation_FreesTheTableAgain()
    {
        var id = await ReserveTable("S11");
        (await _client.PostAsync($"/api/admin/reservations/{id}/confirm", null)).EnsureSuccessStatusCode();
        Assert.Equal("taken", await TableStatus("S11"));

        var res = await _client.PostAsync($"/api/admin/reservations/{id}/cancel", null);

        res.EnsureSuccessStatusCode();
        Assert.Equal("free", await TableStatus("S11"));
    }

    [Fact]
    public async Task Cancelling_PendingReservation_ReturnsBadRequest()
    {
        var id = await ReserveTable("S12");

        var res = await _client.PostAsync($"/api/admin/reservations/{id}/cancel", null);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}
