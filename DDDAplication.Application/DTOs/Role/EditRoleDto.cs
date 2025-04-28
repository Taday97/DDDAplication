using System.ComponentModel.DataAnnotations;

namespace DDDAplication.Application.DTOs.Rol
{
    public class EditRoleDto
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
