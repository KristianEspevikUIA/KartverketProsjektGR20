namespace FirstWebApplication1.Models
{
    /// <summary>
    /// Presentation model for listing users in the admin area. Contains Identity user data plus roles
    /// and organization claim so the view can display and filter information.
    /// </summary>
    public class UsersViewModel
    {
        /// <summary>
        /// Identity user identifier used for edit/delete routes.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// User email used as username.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Roles assigned to the user (Admin/Pilot/Caseworker) for authorization checks.
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// Optional organization claim associated with the user.
        /// </summary>
        public string? Organization { get; set; }
    }
}
