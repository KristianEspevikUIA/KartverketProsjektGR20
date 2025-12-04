using System;
using System.Collections.Generic;
using System.Linq;

namespace FirstWebApplication1.Models
{
    public class ObstacleListViewModel
    {
        // Samling av hindere som vises i listen etter filtrering/søk
        public IEnumerable<ObstacleData> Obstacles { get; set; } = Enumerable.Empty<ObstacleData>();

        public string? StatusFilter { get; set; }

        // Inndata fra søkefilteret i UI
        public string? SearchTerm { get; set; }
        public DateTime? StartDate { get; set; } // Fra-dato
        public DateTime? EndDate { get; set; }   // Til-dato
        public double? MinHeight { get; set; }
        public double? MaxHeight { get; set; }

        // Filtrering på type og organisasjon
        public string? ObstacleTypeFilter { get; set; }
        public string? OrganizationFilter { get; set; }

        // Valgbar liste i UI for å filtrere på organisasjon
        public List<string> AvailableOrganizations { get; set; } =
            new List<string> { "Luftforsvaret", "Norsk Luftambulanse", "Politiets helikoptertjeneste" };

        // Dynamisk sidetittel basert på valgt statusfilter
        public string Title => StatusFilter switch
        {
            "Approved" => "Approved Obstacles",
            "Declined" => "Rejected Obstacles",
            "Pending" => "Pending Obstacles",
            _ => "Obstacle Reports"
        };

        // Kort beskrivelse i toppen av listen, også dynamisk
        public string Description => StatusFilter switch
        {
            "Approved" => "Obstacles that have been reviewed and approved.",
            "Declined" => "Obstacles that were rejected during review.",
            "Pending" => "Obstacles waiting for review.",
            _ => "Manage and view all registered obstacles"
        };
    }
}
