using AutoMapper;
using DDDAplication.Application.DTOs;
using DDDAplication.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DDDAplication.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _rolService;
        private readonly IMapper _mapper;

        public RoleController(IRoleService rolService, IMapper mapper)
        {
            _rolService = rolService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _rolService.GetAllAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { Message = "Role ID is required." });
            }
            var role = await _rolService.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { Message = $"Role with ID {id} not found." });
            }
            return Ok(role);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] RoleDto rolCreateDto)
        {
            if (rolCreateDto == null)
            {
                return BadRequest(new { Message = "Role data is required." });
            }

            var createdRole = await _rolService.AddAsync(rolCreateDto);
            return CreatedAtAction(nameof(GetRoleById), new { id = createdRole.Id }, createdRole);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] RoleDto rolUpdateDto)
        {
            if (rolUpdateDto == null)
            {
                return BadRequest(new { Message = "Role data is required." });
            }

            if (id != rolUpdateDto.Id)
            {
                return BadRequest(new { Message = "The ID in the URL does not match the ID in the request body." });
            }

            var existingRole = await _rolService.GetByIdAsync(id);
            if (existingRole == null)
            {
                return NotFound(new { Message = $"Role with ID {id} not found." });
            }

            var updatedRole = await _rolService.Update(rolUpdateDto);
            return Ok(updatedRole);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var existingRole = await _rolService.GetByIdAsync(id);
            if (existingRole == null)
            {
                return NotFound(new { Message = $"Role with ID {id} not found." });
            }

            var deletedRole = await _rolService.Delete(id);
            return Ok(deletedRole);
        }
    }
}
