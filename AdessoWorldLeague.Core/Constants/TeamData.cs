using AdessoWorldLeague.Core.Enums;

namespace AdessoWorldLeague.Core.Constants;

public static class TeamData
{
    public static readonly IReadOnlyList<(string Name, Country Country)> AllTeams = new List<(string, Country)>
    {
        ("Adesso İstanbul", Country.Turkiye),
        ("Adesso Ankara", Country.Turkiye),
        ("Adesso İzmir", Country.Turkiye),
        ("Adesso Antalya", Country.Turkiye),

        ("Adesso Berlin", Country.Almanya),
        ("Adesso Frankfurt", Country.Almanya),
        ("Adesso Münih", Country.Almanya),
        ("Adesso Dortmund", Country.Almanya),

        ("Adesso Paris", Country.Fransa),
        ("Adesso Marsilya", Country.Fransa),
        ("Adesso Nice", Country.Fransa),
        ("Adesso Lyon", Country.Fransa),

        ("Adesso Amsterdam", Country.Hollanda),
        ("Adesso Rotterdam", Country.Hollanda),
        ("Adesso Lahey", Country.Hollanda),
        ("Adesso Eindhoven", Country.Hollanda),

        ("Adesso Lisbon", Country.Portekiz),
        ("Adesso Porto", Country.Portekiz),
        ("Adesso Braga", Country.Portekiz),
        ("Adesso Coimbra", Country.Portekiz),

        ("Adesso Roma", Country.Italya),
        ("Adesso Milano", Country.Italya),
        ("Adesso Venedik", Country.Italya),
        ("Adesso Napoli", Country.Italya),

        ("Adesso Sevilla", Country.Ispanya),
        ("Adesso Madrid", Country.Ispanya),
        ("Adesso Barselona", Country.Ispanya),
        ("Adesso Granada", Country.Ispanya),

        ("Adesso Brüksel", Country.Belcika),
        ("Adesso Brugge", Country.Belcika),
        ("Adesso Gent", Country.Belcika),
        ("Adesso Anvers", Country.Belcika)
    };

    public static readonly string[] GroupNames = ["A", "B", "C", "D", "E", "F", "G", "H"];

    public static readonly int[] AllowedGroupCounts = [4, 8];
}
