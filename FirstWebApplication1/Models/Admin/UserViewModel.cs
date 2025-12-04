namespace FirstWebApplication1.Models
{
    public class UsersViewModel
    {
        public string Id { get; set; } = string.Empty; // Unik bruker-ID som vises i adminpanelet
        public string Email { get; set; } = string.Empty; // Brukerens e-postadresse

        public List<string> Roles { get; set; } = new List<string>(); // Liste over brukerens roller
        public string? Organization { get; set; } // Organisasjon hentet fra brukerens claim (kan være null)
    }
}
