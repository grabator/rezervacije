using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Rezervacije.Api.Tests;

/// <summary>
/// Pokrece pravu app (Program.cs) nad izolovanom SQLite bazom (jedan fajl po
/// test klasi), tako da testovi vjezbaju stvarno ponasanje - ukljucujuci prave
/// EF Core transakcije koje stite od duple rezervacije.
/// </summary>
public class TestApiFactory : WebApplicationFactory<Program>
{
    public const string AdminPassword = "test-admin-pw";
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"rezervacije-test-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development"); // izbjegava fail-fast provjeru za AdminPassword
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_dbPath}");
        builder.UseSetting("AdminPassword", AdminPassword);
        // Testovi salju vise rezervacija u sekundi nego sto bi ijedan pravi gost ikad poslao -
        // podigni rate-limit da testovi ne padaju na 429 umjesto na ocekivani status.
        builder.UseSetting("RateLimiting:ReservationPermitLimit", "1000");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try { File.Delete(_dbPath); } catch { /* best effort cleanup */ }
    }
}
