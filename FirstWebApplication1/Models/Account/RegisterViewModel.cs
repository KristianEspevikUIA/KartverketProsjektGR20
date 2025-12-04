using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication1.Models.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please select a role")]
        [Display(Name = "Role")] // Vises som etikett i registreringsskjemaet
        public string Role { get; set; } = string.Empty;

        [Required]
        [EmailAddress] // Validerer at e-posten følger korrekt format
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)] // Enkle passordregler
        [DataType(DataType.Password)] // UI håndterer feltet som sensitivt
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")] // Sikrer match ved registrering
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
