namespace FirstWebApplication1.Models
{
    /// <summary>
    /// ViewModel for the initial obstacle type selection step (MVC View binding). Holds the user's choice
    /// so the controller can enforce PRG and prefill downstream forms.
    /// </summary>
    public class ObstacleTypeViewModel
    {
        /// <summary>
        /// Selected obstacle type value coming from the form radio buttons.
        /// </summary>
        public string? SelectedType { get; set; }
    }

    /// <summary>
    /// Static helper exposing the available obstacle types for UI rendering. Centralized list prevents
    /// hard-coded strings in views and keeps options consistent with validation.
    /// </summary>
    public static class ObstacleTypes
    {
        /// <summary>
        /// Available obstacle type options with display metadata for cards/icons in the view.
        /// </summary>
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

    /// <summary>
    /// DTO for representing a single obstacle type option in UI drop-downs or cards.
    /// </summary>
    public class ObstacleTypeOption
    {
        /// <summary>
        /// Value posted back to the controller.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Human-friendly display label.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// FontAwesome icon class used by the view.
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// Short description rendered under the icon to guide user choice.
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
