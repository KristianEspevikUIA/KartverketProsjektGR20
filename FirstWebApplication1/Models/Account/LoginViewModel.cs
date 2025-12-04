using System.ComponentModel.DataAnnotations;
namespace FirstWebApplication1.Models.Account
{

    public class LoginViewModel
    {
        [Required]
        [EmailAddress] // Sikrer at brukeren logger inn med en gyldig e-postadresse
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)] // Marker som passordfelt i UI
        public string Password { get; set; }

        [Display(Name = "Remember me?")] // Vist navn i innloggingsskjemaet
        public bool RememberMe { get; set; }
    }

}
