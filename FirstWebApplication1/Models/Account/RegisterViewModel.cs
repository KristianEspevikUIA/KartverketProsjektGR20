using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication1.Models.Account
{
    /// <summary>
    /// ViewModel for user registration. DataAnnotations drive ModelState validation so the controller can
    /// safely create Identity users and assign roles.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// Role selected by the registering user (Admin/Pilot/Caseworker). Admin is restricted in controller.
        /// </summary>
        [Required(ErrorMessage = "Please select a role")]
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Email used as the username within Identity.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Password supplied by the user; Identity handles hashing/salting.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Confirmation of the password to avoid typos before committing to the database.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
