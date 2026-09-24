using System.Net;
using Xunit;

namespace Rezervacije.Api.Tests;

public class AdminAuthTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;

    public AdminAuthTests(TestApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AdminEndpoint_WithoutPassword_Returns401()
    {
        var res = await _client.GetAsync("/api/admin/reservations");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_WithWrongPassword_Returns401()
    {
        _client.DefaultRequestHeaders.Add("X-Admin-Password", "pogresna-lozinka");
        var res = await _client.GetAsync("/api/admin/reservations");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_WithCorrectPassword_Returns200()
    {
        _client.DefaultRequestHeaders.Add("X-Admin-Password", TestApiFactory.AdminPassword);
        var res = await _client.GetAsync("/api/admin/reservations");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
}
