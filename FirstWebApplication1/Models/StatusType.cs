using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication1.Models
{
    public class StatusType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }
    }
}
