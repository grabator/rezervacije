using Rezervacije.Api.Models;

namespace Rezervacije.Api.Data;

public static class SeedData
{
    public static void EnsureSeeded(AppDbContext db)
    {
        if (db.Venues.Any()) return;

        var venue = new Venue
        {
            Slug = "exclusive",
            Name = "Exclusive Caffe Lounge",
            Subtitle = "Rezervacija stola",
            Instagram = "https://www.instagram.com/exclusivecaffe.lounge",
        };
        db.Venues.Add(venue);
        db.SaveChanges();

        var ev = new VenueEvent
        {
            Id = "poljska-bih",
            VenueId = venue.Id,
            Title = "Poljska vs Bosna i Hercegovina",
            Subtitle = "Utakmica na velikim ekranima",
            StartsAt = new DateTime(2026, 9, 25, 20, 45, 0, DateTimeKind.Unspecified),
            FloorWidth = 900,
            FloorHeight = 620,
        };
        db.Events.Add(ev);
        db.SaveChanges();

        db.Packages.AddRange(
            new TablePackage { Id = "kibla-tuborg", EventId = ev.Id, Name = "Kibla Tuborg", Persons = 4, Price = 50, Description = "6 × Tuborg, 1 × shisha, grickalice" },
            new TablePackage { Id = "kibla-somersby", EventId = ev.Id, Name = "Kibla Somersby", Persons = 4, Price = 50, Description = "6 × Somersby bezalkoholni, 1 × shisha, grickalice" },
            new TablePackage { Id = "kibla-redbull", EventId = ev.Id, Name = "Kibla Red Bull", Persons = 4, Price = 60, Description = "6 × Red Bull, 1 × shisha, grickalice" },
            new TablePackage { Id = "kibla-fast", EventId = ev.Id, Name = "Kibla Fast", Persons = 4, Price = 45, Description = "6 × Fast, 1 × shisha, grickalice" }
        );

        db.FloorElements.AddRange(
            new FloorElementEntity { EventId = ev.Id, Kind = "bar", X = 10, Y = 81, W = 140, H = 30, Text = "Šank" },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 8, Y = 7, W = 140, H = 30, Text = "Shisha" },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 324, Y = 105, W = 110, H = 80, Text = "WC" },
            new FloorElementEntity { EventId = ev.Id, Kind = "tv", X = 48, Y = 410, W = 50, H = 6, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "tv", X = 880, Y = 483, W = 6, H = 60, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "entrance", X = 173, Y = 420, W = 36, H = 20, Text = "Ulaz" },
            new FloorElementEntity { EventId = ev.Id, Kind = "entrance", X = 176, Y = 560, W = 36, H = 20, Text = "Bašta" },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 207, Y = 110, W = 90, H = 30, Text = "Igrice" },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 772, Y = 258, W = 90, H = 30, Text = "Igrice" },
            new FloorElementEntity { EventId = ev.Id, Kind = "tv", X = 278, Y = 356, W = 50, H = 6, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "tv", X = 598, Y = 356, W = 50, H = 6, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "entrance", X = 4, Y = 440, W = 892, H = 2, Text = "" }
        );

        (string key, int x, int y, int seats, string shape, string status)[] tables =
        [
            ("S1", 66, 165, 4, "round", "free"),
            ("S2", 67, 235, 3, "round", "free"),
            ("S3", 71, 373, 3, "round", "taken"),
            ("S4", 71, 305, 3, "square", "free"),
            ("S5", 252, 159, 4, "round", "free"),
            ("S6", 494, 235, 4, "round", "pending"),
            ("S7", 588, 235, 4, "round", "free"),
            ("S8", 667, 236, 4, "round", "free"),
            ("S9", 748, 237, 4, "round", "taken"),
            ("S10", 830, 236, 4, "round", "free"),
            ("S11", 382, 358, 4, "round", "free"),
            ("S12", 526, 360, 4, "round", "free"),
            ("S13", 681, 360, 4, "round", "taken"),
            ("S14", 830, 361, 4, "round", "free"),
            ("S15", 263, 239, 4, "round", "free"),
            ("S16", 73, 460, 4, "round", "free"),
            ("S17", 76, 562, 4, "round", "free"),
            ("S18", 311, 461, 4, "round", "free"),
            ("S19", 446, 460, 4, "round", "pending"),
            ("S20", 589, 460, 4, "round", "free"),
            ("S21", 727, 461, 4, "round", "free"),
            ("S22", 316, 571, 4, "round", "free"),
            ("S23", 455, 574, 4, "round", "free"),
            ("S24", 591, 571, 4, "round", "taken"),
            ("S25", 727, 574, 4, "round", "free"),
        ];
        foreach (var (key, x, y, seats, shape, status) in tables)
        {
            db.Tables.Add(new FloorTableEntity
            {
                EventId = ev.Id,
                TableKey = key,
                Label = key,
                X = x,
                Y = y,
                Seats = seats,
                Shape = shape,
                Status = status,
            });
        }

        db.SaveChanges();
    }
}
