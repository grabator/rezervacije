using System.Net;
using Xunit;

namespace Rezervacije.Api.Tests;

/// <summary>
/// Zaseban od AdminAuthTests jer koristi svoj TestApiFactory (izolovan brojac
/// pogresnih pokusaja) - ne smije uticati na druge testove koji provjeravaju
/// samo obican 401.
/// </summary>
public class AdminLockoutTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;

    public AdminLockoutTests(TestApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FiveWrongPasswords_LocksOutEvenTheCorrectPasswordFor5Minutes()
    {
        for (var i = 0; i < 5; i++)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, "/api/admin/reservations");
            req.Headers.Add("X-Admin-Password", "pogresna-lozinka");
            var res = await _client.SendAsync(req);
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        using var lockedReq = new HttpRequestMessage(HttpMethod.Get, "/api/admin/reservations");
        lockedReq.Headers.Add("X-Admin-Password", TestApiFactory.AdminPassword);
        var lockedRes = await _client.SendAsync(lockedReq);

        Assert.Equal((HttpStatusCode)429, lockedRes.StatusCode);
    }
}
