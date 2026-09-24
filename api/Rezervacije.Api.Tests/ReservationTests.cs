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

    private static object ValidRequest(string tableId, string? fullName = null, string? email = null, string? phone = null) => new
    {
        eventId = "poljska-bih",
        tableId,
        fullName = fullName ?? "Test Gost",
        phone = phone ?? "+38761234567",
        email = email ?? "gost@test.com",
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
    [InlineData("", "gost@test.com", "+38761234567")]      // prazno ime
    [InlineData("ab", "gost@test.com", "+38761234567")]    // prekratko ime
    [InlineData("Test Gost", "nije-email", "+38761234567")] // neispravan email
    [InlineData("Test Gost", "gost@test.com", "123")]       // prekratak telefon
    public async Task Reserving_WithInvalidInput_ReturnsBadRequest(string name, string email, string phone)
    {
        var res = await _client.PostAsJsonAsync("/api/reservations", ValidRequest("S5", name, email, phone));
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}
