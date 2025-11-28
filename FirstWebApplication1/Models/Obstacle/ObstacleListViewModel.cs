using System;
using System.Collections.Generic;
using System.Linq;

namespace FirstWebApplication1.Models
{
    public class ObstacleListViewModel
    {
        // collection of obstacles
        public IEnumerable<ObstacleData> Obstacles { get; set; } = Enumerable.Empty<ObstacleData>();

        public string? StatusFilter { get; set; }

        // properties for filtering
        public string? SearchTerm { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // property for type filtering
        public string? ObstacleTypeFilter { get; set; }

         public string? OrganizationFilter { get; set; }

        // dynamic title and description based on status filter
        public string Title => StatusFilter switch
        {
            "Approved" => "Approved Obstacles",
            "Declined" => "Rejected Obstacles",
            "Pending" => "Pending Obstacles",
            _ => "Obstacle Reports"
        };

        // dynamic description based on status filter
        public string Description => StatusFilter switch
        {
            "Approved" => "Obstacles that have been reviewed and approved.",
            "Declined" => "Obstacles that were rejected during review.",
            "Pending" => "Obstacles waiting for review.",
            _ => "Manage and view all registered obstacles"
        };
    }
}