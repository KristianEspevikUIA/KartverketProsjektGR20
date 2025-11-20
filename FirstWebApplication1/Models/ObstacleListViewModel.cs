using System.Linq;

namespace FirstWebApplication1.Models
{
    public class ObstacleListViewModel
    {
        public IEnumerable<ObstacleData> Obstacles { get; set; } = Enumerable.Empty<ObstacleData>();

        public string? StatusFilter { get; set; }

        public string Title => StatusFilter switch
        {
            "Approved" => "Approved Obstacles",
            "Declined" => "Rejected Obstacles",
            "Pending" => "Pending Obstacles",
            _ => "Obstacle Reports"
        };

        public string Description => StatusFilter switch
        {
            "Approved" => "Obstacles that have been reviewed and approved.",
            "Declined" => "Obstacles that were rejected during review.",
            "Pending" => "Obstacles waiting for review.",
            _ => "Manage and view all registered obstacles"
        };
    }
}