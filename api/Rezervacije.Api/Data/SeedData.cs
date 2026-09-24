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
            FloorWidth = 764,
            FloorHeight = 545,
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
            new FloorElementEntity { EventId = ev.Id, Kind = "bar", X = 4, Y = 86, W = 151, H = 33, Text = "Šank" },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 8, Y = 7, W = 107, H = 35, Text = "Shisha" },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 324, Y = 24, W = 141, H = 93, Text = "WC" },
            new FloorElementEntity { EventId = ev.Id, Kind = "tv", X = 11, Y = 371, W = 126, H = 19, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "entrance", X = 188, Y = 374, W = 36, H = 20, Text = "Ulaz" },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 193, Y = 128, W = 60, H = 35, Text = "Igrice" },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 667, Y = 216, W = 41, H = 61, Text = "Igrice" },
            new FloorElementEntity { EventId = ev.Id, Kind = "tv", X = 308, Y = 309, W = 16, H = 70, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "tv", X = 535, Y = 316, W = 17, H = 66, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "entrance", X = 4, Y = 397, W = 756, H = 2, Text = "" },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 501, Y = 21, W = 247, H = 103, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "sofa", X = 381, Y = 139, W = 374, H = 26, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "entrance", X = 187, Y = 517, W = 36, H = 20, Text = "Bašta" },
            new FloorElementEntity { EventId = ev.Id, Kind = "tv", X = 734, Y = 412, W = 19, H = 100, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 246, Y = 292, W = 41, H = 94, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 197, Y = 171, W = 92, H = 12, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 310, Y = 125, W = 10, H = 59, Text = null }
        );

        (string key, int x, int y, int size, int seats, string shape, string status)[] tables =
        [
            ("S1", 71, 165, 42, 4, "square", "free"),
            ("S2", 72, 246, 42, 3, "square", "free"),
            ("S3", 71, 340, 42, 3, "square", "taken"),
            ("S4", 71, 294, 40, 3, "square", "free"),
            ("S5", 280, 143, 42, 4, "square", "free"),
            ("S6", 426, 194, 44, 4, "round", "pending"),
            ("S7", 492, 194, 48, 4, "round", "free"),
            ("S8", 567, 193, 48, 4, "round", "free"),
            ("S9", 639, 192, 48, 4, "round", "taken"),
            ("S10", 734, 227, 46, 4, "round", "free"),
            ("S11", 355, 350, 48, 4, "round", "free"),
            ("S12", 460, 350, 48, 4, "round", "free"),
            ("S13", 583, 351, 48, 4, "round", "taken"),
            ("S14", 718, 353, 48, 4, "round", "free"),
            ("S15", 258, 219, 42, 4, "square", "free"),
            ("S16", 69, 435, 48, 4, "round", "free"),
            ("S17", 70, 505, 48, 4, "round", "free"),
            ("S18", 269, 425, 48, 4, "round", "free"),
            ("S19", 371, 426, 48, 4, "round", "pending"),
            ("S20", 481, 427, 48, 4, "round", "free"),
            ("S21", 578, 428, 48, 4, "round", "free"),
            ("S22", 269, 511, 48, 4, "round", "free"),
            ("S23", 371, 510, 48, 4, "round", "free"),
            ("S24", 476, 514, 48, 4, "round", "taken"),
            ("S25", 573, 514, 48, 4, "round", "free"),
            ("S26", 668, 514, 48, 4, "round", "free"),
            ("S27", 666, 429, 48, 4, "round", "free"),
        ];
        foreach (var (key, x, y, size, seats, shape, status) in tables)
        {
            db.Tables.Add(new FloorTableEntity
            {
                EventId = ev.Id,
                TableKey = key,
                Label = key,
                X = x,
                Y = y,
                Size = size,
                Seats = seats,
                Shape = shape,
                Status = status,
            });
        }

        db.SaveChanges();
    }
}
