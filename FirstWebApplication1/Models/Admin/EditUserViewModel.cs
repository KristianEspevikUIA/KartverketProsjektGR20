using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication1.Models.Admin
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty; // Brukerens unike identifikator
        public string Email { get; set; } = string.Empty; // Vises i admin-panelet for referanse

        public string? SelectedRole { get; set; } // Rollen admin velger i dropdown
        public List<string> CurrentRoles { get; set; } = new List<string>(); // Roller brukeren har nå
        public List<string> AvailableRoles { get; set; } = new List<string>(); // Roller admin kan velge mellom

        public string? Organization { get; set; } // Nåværende organisasjon (claim)

        // Fast liste med organisasjoner admin kan velge fra
        public List<string> AvailableOrganizations { get; set; } = new List<string>
        {
            "Luftforsvaret",
            "Norsk Luftambulanse",
            "Politiets helikoptertjeneste"
        };
    }
}
