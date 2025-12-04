using System.ComponentModel.DataAnnotations;
namespace FirstWebApplication1.Models.Account
{
    /// <summary>
    /// ViewModel backing the login form. Bound by the AccountController and validated through
    /// DataAnnotations before authentication is attempted.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// User's email address used as username for Identity authentication.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Plain-text password entered by the user; hashed comparison is performed by Identity.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Persists the authentication cookie beyond the session when true.
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
