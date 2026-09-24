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
            FloorWidth = 980,
            FloorHeight = 715,
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
            new FloorElementEntity { EventId = ev.Id, Kind = "bar", X = 5, Y = 108, W = 189, H = 41, Text = "Šank" },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 10, Y = 9, W = 134, H = 44, Text = "Shisha" },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 405, Y = 30, W = 176, H = 116, Text = "WC" },
            new FloorElementEntity { EventId = ev.Id, Kind = "tv", X = 14, Y = 464, W = 158, H = 24, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "entrance", X = 235, Y = 468, W = 45, H = 25, Text = "Ulaz" },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 241, Y = 160, W = 75, H = 44, Text = "Igrice" },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 834, Y = 270, W = 51, H = 76, Text = "Igrice" },
            new FloorElementEntity { EventId = ev.Id, Kind = "tv", X = 385, Y = 386, W = 20, H = 88, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "tv", X = 669, Y = 395, W = 21, H = 83, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "entrance", X = 5, Y = 496, W = 945, H = 3, Text = "" },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 626, Y = 26, W = 309, H = 129, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "sofa", X = 476, Y = 174, W = 468, H = 33, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "entrance", X = 234, Y = 646, W = 45, H = 25, Text = "Bašta" },
            new FloorElementEntity { EventId = ev.Id, Kind = "tv", X = 918, Y = 515, W = 24, H = 125, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 308, Y = 365, W = 51, H = 118, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 246, Y = 214, W = 115, H = 15, Text = null },
            new FloorElementEntity { EventId = ev.Id, Kind = "label", X = 388, Y = 156, W = 13, H = 74, Text = null }
        );

        (string key, int x, int y, int size, int seats, string shape, string status)[] tables =
        [
            ("S1", 89, 206, 56, 4, "square", "free"),
            ("S2", 90, 308, 56, 3, "square", "free"),
            ("S3", 89, 425, 56, 3, "square", "taken"),
            ("S4", 89, 368, 54, 3, "square", "free"),
            ("S5", 350, 179, 56, 4, "square", "free"),
            ("S6", 533, 243, 59, 4, "round", "pending"),
            ("S7", 615, 243, 65, 4, "round", "free"),
            ("S8", 709, 241, 65, 4, "round", "free"),
            ("S9", 799, 240, 65, 4, "round", "taken"),
            ("S10", 918, 284, 63, 4, "round", "free"),
            ("S11", 444, 438, 65, 4, "round", "free"),
            ("S12", 575, 438, 65, 4, "round", "free"),
            ("S13", 729, 439, 65, 4, "round", "taken"),
            ("S14", 898, 441, 65, 4, "round", "free"),
            ("S15", 323, 274, 56, 4, "square", "free"),
            ("S16", 86, 544, 65, 4, "round", "free"),
            ("S17", 88, 631, 65, 4, "round", "free"),
            ("S18", 336, 531, 65, 4, "round", "free"),
            ("S19", 464, 533, 65, 4, "round", "pending"),
            ("S20", 601, 534, 65, 4, "round", "free"),
            ("S21", 723, 535, 65, 4, "round", "free"),
            ("S22", 336, 639, 65, 4, "round", "free"),
            ("S23", 464, 638, 65, 4, "round", "free"),
            ("S24", 595, 643, 65, 4, "round", "taken"),
            ("S25", 716, 643, 65, 4, "round", "free"),
            ("S26", 835, 643, 65, 4, "round", "free"),
            ("S27", 833, 536, 65, 4, "round", "free"),
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
