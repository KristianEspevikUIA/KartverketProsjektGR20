namespace FirstWebApplication1.Models
{
    public class ObstacleTypeViewModel
    {
        public string? SelectedType { get; set; } // Valget brukeren gjør i steg 1 av registrering
    }

    public static class ObstacleTypes
    {
        // Fast definert liste over hinder-typer brukt i UI for valg, ikon og beskrivelse
        public static readonly List<ObstacleTypeOption> Types = new()
        {
            new ObstacleTypeOption
            {
                Value = "Crane",
                DisplayName = "Crane",
                Icon = "fa-solid fa-tower-cell",
                Description = "Construction cranes and lifting equipment"
            },
            new ObstacleTypeOption
            {
                Value = "Tower",
                DisplayName = "Tower",
                Icon = "fa-solid fa-broadcast-tower",
                Description = "Communication and broadcast towers"
            },
            new ObstacleTypeOption
            {
                Value = "Building",
                DisplayName = "Building",
                Icon = "fa-solid fa-building",
                Description = "Tall buildings and structures"
            },
            new ObstacleTypeOption
            {
                Value = "Mast",
                DisplayName = "Mast",
                Icon = "fa-solid fa-signal",
                Description = "Radio masts and antenna structures"
            },
            new ObstacleTypeOption
            {
                Value = "Windmill",
                DisplayName = "Windmill",
                Icon = "fa-solid fa-wind",
                Description = "Wind turbines and windmills"
            },
            new ObstacleTypeOption
            {
                Value = "Other",
                DisplayName = "Other",
                Icon = "fa-solid fa-question",
                Description = "Other types of obstacles"
            }
        };
    }

    public class ObstacleTypeOption
    {
        public string Value { get; set; } = string.Empty; // Intern verdi lagret i databasen
        public string DisplayName { get; set; } = string.Empty; // Tekst vist i UI
        public string Icon { get; set; } = string.Empty; // FontAwesome-ikon som representerer typen
        public string Description { get; set; } = string.Empty; // Korte forklaringer i UI
    }
}
