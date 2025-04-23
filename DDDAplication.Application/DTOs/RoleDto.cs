using System.ComponentModel.DataAnnotations;

namespace DDDAplication.Application.DTOs
{
    public class RoleDto
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
