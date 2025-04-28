using System.ComponentModel.DataAnnotations;

namespace DDDAplication.Application.DTOs.Rol
{
    public class RoleDto
    {

        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
