using System.ComponentModel.DataAnnotations;

namespace DDDAplication.Application.DTOs.Rol
{
    public class CreateRoleDto
    {
        [Required]
        public string Name { get; set; }
    }
}
