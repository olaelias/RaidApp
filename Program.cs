using System.Text.Json;

namespace RaidApp;

class Program
{
    static void Main(string[] args)
    {
        string appFolder = AppContext.BaseDirectory;

        string ActiveRaidFile =
            Path.Combine(appFolder, "active_raid.json");

        string LogFile =
            Path.Combine(appFolder, "raid_log.json");



        //const string ActiveRaidFile = "active_raid.json";
        //const string LogFile = "raid_log.json";

        while (true)
        {
            Console.Clear();

            Raid? activeRaid = LoadActiveRaid();

            if (activeRaid != null)
            {
                ShowActiveRaidMenu(activeRaid);
            }
            else
            {
                ShowMainMenu();
            }
        }

        void ShowMainMenu()
        {
            Console.WriteLine("================================");
            Console.WriteLine("       RAID JOURNAL");
            Console.WriteLine("================================");
            Console.WriteLine();
            Console.WriteLine("1. Starta ny Raid");
            Console.WriteLine("2. Loggbok");
            Console.WriteLine("0. Avsluta");
            Console.WriteLine();
            Console.Write("> ");

            switch (Console.ReadLine())
            {
                case "1":
                    StartRaid();
                    break;

                case "2":
                    ShowLogbook();
                    break;

                case "0":
                    Environment.Exit(0);
                    break;
            }
        }

        void StartRaid()
        {
            Console.Clear();

            Console.WriteLine("=== NY RAID ===");
            Console.WriteLine();

            Console.Write("Mål: ");
            string goal = Console.ReadLine() ?? "";

            Console.Write("Syfte: ");
            string purpose = Console.ReadLine() ?? "";

            Raid raid = new()
            {
                Goal = goal,
                Purpose = purpose,
                Started = DateTime.Now
            };

            SaveActiveRaid(raid);

            Console.WriteLine();
            Console.WriteLine("Raid startad.");
            Pause();
        }

        void ShowActiveRaidMenu(Raid raid)
        {
            Console.WriteLine("================================");
            Console.WriteLine("        RAID PÅGÅR");
            Console.WriteLine("================================");
            Console.WriteLine();

            Console.WriteLine($"Startad : {raid.Started:g}");
            Console.WriteLine($"Mål     : {raid.Goal}");
            Console.WriteLine($"Syfte   : {raid.Purpose}");

            Console.WriteLine();
            Console.WriteLine("1. Avsluta Raid");
            Console.WriteLine("2. Loggbok");
            Console.WriteLine("0. Avsluta program");
            Console.WriteLine();

            Console.Write("> ");

            switch (Console.ReadLine())
            {
                case "1":
                    CompleteRaid(raid);
                    break;

                case "2":
                    ShowLogbook();
                    break;
                case "0":
                    Environment.Exit(0);
                    break;
            }
        }

        void CompleteRaid(Raid raid)
        {
            Console.Clear();

            Console.WriteLine("=== AVSLUTA RAID ===");
            Console.WriteLine();
            Console.WriteLine("Kommentar krävs.");
            Console.WriteLine();

            string comment = "";

            while (string.IsNullOrWhiteSpace(comment))
            {
                Console.Write("Kommentar: ");
                comment = Console.ReadLine() ?? "";
            }

            raid.Comment = comment;
            raid.Ended = DateTime.Now;

            var log = LoadLog();

            log.Add(raid);

            SaveLog(log);

            if (File.Exists(ActiveRaidFile))
                File.Delete(ActiveRaidFile);

            Console.WriteLine();
            Console.WriteLine("Raid avslutad.");
            Pause();
        }

        void ShowLogbook()
        {
            while (true)
            {
                Console.Clear();

                var log = LoadLog()
                    .OrderByDescending(x => x.Started)
                    .ToList();

                if (!log.Any())
                {
                    Console.WriteLine("Loggboken är tom.");
                    Pause();
                    return;
                }

                var years = log
                    .Select(x => x.Started.Year)
                    .Distinct()
                    .OrderByDescending(x => x)
                    .ToList();

                Console.WriteLine("=== ÅR ===");
                Console.WriteLine();

                for (int i = 0; i < years.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {years[i]}");
                }

                Console.WriteLine();
                Console.WriteLine("0. Tillbaka");
                Console.Write("> ");

                if (!int.TryParse(Console.ReadLine(), out int yearChoice))
                    continue;

                if (yearChoice == 0)
                    return;

                if (yearChoice < 1 || yearChoice > years.Count)
                    continue;

                int selectedYear = years[yearChoice - 1];

                ShowMonths(selectedYear);
            }
        }

        void ShowMonths(int year)
        {
            while (true)
            {
                Console.Clear();

                var log = LoadLog()
                    .Where(x => x.Started.Year == year)
                    .ToList();

                var months = log
                    .Select(x => x.Started.Month)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                Console.WriteLine($"=== {year} ===");
                Console.WriteLine();

                for (int i = 0; i < months.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {MonthName(months[i])}");
                }

                Console.WriteLine();
                Console.WriteLine("0. Tillbaka");
                Console.Write("> ");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                    continue;

                if (choice == 0)
                    return;

                if (choice < 1 || choice > months.Count)
                    continue;

                ShowEntries(year, months[choice - 1]);
            }
        }

        void ShowEntries(int year, int month)
        {
            while (true)
            {
                Console.Clear();

                var entries = LoadLog()
                    .Where(x => x.Started.Year == year &&
                                x.Started.Month == month)
                    .OrderBy(x => x.Started)
                    .ToList();

                Console.WriteLine($"=== {MonthName(month)} {year} ===");
                Console.WriteLine();

                for (int i = 0; i < entries.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {entries[i].Started:d}");
                    Console.WriteLine($"   Mål: {entries[i].Goal}");
                    Console.WriteLine($"   Syfte: {entries[i].Purpose}");
                    Console.WriteLine();
                }

                Console.WriteLine("0. Tillbaka");
                Console.Write("> ");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                    continue;

                if (choice == 0)
                    return;

                if (choice < 1 || choice > entries.Count)
                    continue;

                ShowEntry(entries[choice - 1]);
            }
        }

        void ShowEntry(Raid raid)
        {
            Console.Clear();

            Console.WriteLine("=== RAID ===");
            Console.WriteLine();

            Console.WriteLine($"Start : {raid.Started}");
            Console.WriteLine($"Slut  : {raid.Ended}");
            Console.WriteLine();

            Console.WriteLine($"Mål:");
            Console.WriteLine(raid.Goal);

            Console.WriteLine();

            Console.WriteLine($"Syfte:");
            Console.WriteLine(raid.Purpose);

            Console.WriteLine();

            Console.WriteLine($"Kommentar:");
            Console.WriteLine(raid.Comment);

            Console.WriteLine();
            Console.WriteLine("1. Redigera kommentar");
            Console.WriteLine("0. Tillbaka");
            Console.Write("> ");

            string? input = Console.ReadLine();

            if (input == "1")
            {
                Console.WriteLine();
                Console.Write("Ny kommentar: ");

                raid.Comment = Console.ReadLine() ?? "";



                var log = LoadLog();

                var target = log.FirstOrDefault(x =>
                    x.Started == raid.Started);

                if (target != null)
                {
                    target.Comment = raid.Comment;
                    SaveLog(log);
                }

                Console.WriteLine();
                Console.WriteLine("Sparat.");
                Pause();
            }
        }

        Raid? LoadActiveRaid()
        {
            if (!File.Exists(ActiveRaidFile))
                return null;

            return JsonSerializer.Deserialize<Raid>(
                File.ReadAllText(ActiveRaidFile));
        }

        void SaveActiveRaid(Raid raid)
        {
            File.WriteAllText(
                ActiveRaidFile,
                JsonSerializer.Serialize(raid,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }));
        }

        List<Raid> LoadLog()
        {
            if (!File.Exists(LogFile))
                return new List<Raid>();

            return JsonSerializer.Deserialize<List<Raid>>(
                       File.ReadAllText(LogFile))
                   ?? new List<Raid>();
        }

        void SaveLog(List<Raid> raids)
        {
            File.WriteAllText(
                LogFile,
                JsonSerializer.Serialize(
                    raids,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }));
        }

        string MonthName(int month)
        {
            return new DateTime(2000, month, 1)
                .ToString("MMMM");
        }

        void Pause()
        {
            Console.WriteLine();
            Console.WriteLine("Tryck Enter...");
            Console.ReadLine();
        }

    }
}

class Raid
{
    public string Goal { get; set; } = "";
    public string Purpose { get; set; } = "";
    public string Comment { get; set; } = "";

    public DateTime Started { get; set; }

    public DateTime? Ended { get; set; }
}
