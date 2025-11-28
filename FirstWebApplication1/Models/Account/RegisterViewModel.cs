using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication1.Models.Account
{
    public class RegisterViewModel
    {
        // Role selection for registration
        [Required(ErrorMessage = "Please select a role")]
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;
// Email address for the new user
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        // Password for the new user
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;
        // Confirmation of the password
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
