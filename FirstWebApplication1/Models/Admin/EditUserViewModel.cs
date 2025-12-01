using Microsoft.AspNetCore.Mvc;

namespace FirstWebApplication1.Models.Admin
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? SelectedRole { get; set; }
        public List<string> CurrentRoles { get; set; } = new List<string>();
        public List<string> AvailableRoles { get; set; } = new List<string>();
        public string? Organization { get; set; }
        public List<string> AvailableOrganizations { get; set; } = new List<string> { "Luftforsvaret", "Norsk Luftambulanse", "Politiets helikoptertjeneste" };
    }
}