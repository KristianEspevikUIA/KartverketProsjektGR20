using System;
using System.Collections.Generic;
using System.Linq;

namespace FirstWebApplication1.Models
{
    /// <summary>
    /// ViewModel used by the Obstacle list/overview views (the "View" in MVC). Holds both the obstacle
    /// collection and the active filter criteria to support PRG and ModelState re-display.
    /// </summary>
    public class ObstacleListViewModel
    {
        /// <summary>
        /// Collection of obstacles retrieved via EF Core. Controllers populate this using AsQueryable to
        /// keep SQL parameterized and protect against injection while allowing sorting/filtering.
        /// </summary>
        public IEnumerable<ObstacleData> Obstacles { get; set; } = Enumerable.Empty<ObstacleData>();

        /// <summary>
        /// Current status filter (Pending/Approved/Declined) coming from query string.
        /// </summary>
        public string? StatusFilter { get; set; }

        /// <summary>
        /// Free-text search term applied against obstacle names.
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Optional start date for submitted date range filtering.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Optional end date for submitted date range filtering.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Minimum obstacle height filter (meters).
        /// </summary>
        public double? MinHeight { get; set; }

        /// <summary>
        /// Maximum obstacle height filter (meters).
        /// </summary>
        public double? MaxHeight { get; set; }

        /// <summary>
        /// Selected obstacle type filter.
        /// </summary>
        public string? ObstacleTypeFilter { get; set; }

        /// <summary>
        /// Organization filter used by admins/caseworkers for reporting.
        /// </summary>
        public string? OrganizationFilter { get; set; }

        /// <summary>
        /// Predefined list of organizations shown in dropdowns to avoid free-text injection and to align with claims.
        /// </summary>
        public List<string> AvailableOrganizations { get; set; } = new List<string> { "Luftforsvaret", "Norsk Luftambulanse", "Politiets helikoptertjeneste" };

        /// <summary>
        /// Title text derived from the current status filter for a user-friendly heading.
        /// </summary>
        public string Title => StatusFilter switch
        {
            "Approved" => "Approved Obstacles",
            "Declined" => "Rejected Obstacles",
            "Pending" => "Pending Obstacles",
            _ => "Obstacle Reports"
        };

        /// <summary>
        /// Description shown in the UI explaining the context of the filtered list.
        /// </summary>
        public string Description => StatusFilter switch
        {
            "Approved" => "Obstacles that have been reviewed and approved.",
            "Declined" => "Obstacles that were rejected during review.",
            "Pending" => "Obstacles waiting for review.",
            _ => "Manage and view all registered obstacles"
        };
    }
}
