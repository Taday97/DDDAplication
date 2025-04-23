using AutoMapper;
using DDDAplication.Application.DTOs;
using DDDAplication.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DDDAplication.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IMapper _mapper;

        public RoleController(IRoleService roleService, IMapper mapper)
        {
            _roleService = roleService;
            _mapper = mapper;
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] RoleDto roleDto)
        {
            if (roleDto == null)
                return BadRequest("Role data cannot be null.");

            var response = await _roleService.AddAsync(roleDto);

            if (!response.Success)
                return BadRequest(response.Message);

            return CreatedAtAction(nameof(GetRoleById), new { id = response.Data.Id }, response.Data);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var response = await _roleService.GetAllAsync();

            if (!response.Success)
                return NotFound(response.Message);


            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Role ID is required.");  
            }
            var response = await _roleService.GetByIdAsync(id);

            if (!response.Success)
                return NotFound(response.Message);

            return Ok(response.Data);
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] RoleDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Role data cannot be null.");
            }
            if (dto.Id != id)
            {
                return BadRequest("Role ID in the request body does not match the ID in the URL.");
            }

            dto.Id = id;
            var response = await _roleService.Update(dto);
            if (!response.Success)
                return BadRequest(response.Message);
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{roleId}")]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                var errorResponse = ApiResponse<RoleDto>.CreateErrorResponse("Role ID is required.");
                return BadRequest(errorResponse.Message);  
            }

            var response = await _roleService.Delete(roleId);

            if (!response.Success)
                return NotFound(response.Message);
            return Ok(response);
        }


    }
}
