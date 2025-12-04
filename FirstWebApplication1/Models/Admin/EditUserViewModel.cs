using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication1.Models.Admin
{
    /// <summary>
    /// ViewModel for editing a user's role and organization claim. Used by AdminController actions and views.
    /// </summary>
    public class EditUserViewModel
    {
        /// <summary>
        /// Identity user id being edited.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Email/username of the account.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Role selected in the form (single role assignment enforced in controller).
        /// </summary>
        public string? SelectedRole { get; set; }

        /// <summary>
        /// Roles currently assigned to the user (for display/reference).
        /// </summary>
        public List<string> CurrentRoles { get; set; } = new List<string>();

        /// <summary>
        /// Roles available for selection, populated from RoleManager to avoid magic strings.
        /// </summary>
        public List<string> AvailableRoles { get; set; } = new List<string>();

        /// <summary>
        /// Organization claim value selected by the admin.
        /// </summary>
        public string? Organization { get; set; }

        /// <summary>
        /// Pre-defined organizations to keep input constrained and consistent with claims-based auth.
        /// </summary>
        public List<string> AvailableOrganizations { get; set; } = new List<string> { "Luftforsvaret", "Norsk Luftambulanse", "Politiets helikoptertjeneste" };
    }
}
