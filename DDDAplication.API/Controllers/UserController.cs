using Azure;
using DDDAplication.Application.DTOs;
using DDDAplication.Application.Interfaces;
using DDDAplication.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DDDAplication.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found.");

            var response = await _userService.GetProfileAsync(userId);

            if (!response.Success)
                return NotFound(response.Message);

            return Ok(response.Data);
        }

        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UserDto dto)
        {
            dto.Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(dto.Id))
                return Unauthorized("User ID not found.");

            var response = await _userService.Update(dto);
            if (!response.Success)
                return NotFound(response.Message);

            return Ok(response.Data);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var response = await _userService.GetAllAsync();
            if (!response.Success)
                return NotFound(response.Message);

            return Ok(response.Data);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("User ID cannot be null or empty.");
            var response = await _userService.GetByIdAsync(id);

            if (!response.Success)
                return NotFound(response.Message); 

            return Ok(response.Data);
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserDto dto)
        {
            if (dto == null)
            {
                return BadRequest("User data cannot be null.");
            }
            if (dto.Id != id)
            {
                return BadRequest("User ID in the request body does not match the ID in the URL.");
            }
            dto.Id = id;

            var response = await _userService.Update(dto);
            if (!response.Success)
                return BadRequest(response.Message);
            return Ok(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                var errorResponse = ApiResponse<RoleDto>.CreateErrorResponse("User ID is required.");
                return BadRequest(errorResponse.Message);
            }
            var response = await _userService.Delete(id);

            if (!response.Success)
                return NotFound(response.Message);

            return Ok(response);
        }



        [HttpPost("{userId}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRolesToUser(string userId, [FromBody] List<string> roles)
        {
            if (roles == null || !roles.Any())
                return BadRequest("Users list cannot be empty.");

            var response = await _userService.AssignRolesToUserAsync(userId, roles);

            if (!response.Success)
                return BadRequest(response.Message);

            return Ok(response);
        }

        [HttpDelete("{userId}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRolesFromUser(string userId, [FromBody] List<string> roles)
        {
            if (roles == null || !roles.Any())
                return BadRequest("Users list cannot be empty.");

            var response = await _userService.RemoveRolesFromUserAsync(userId, roles);

            if (!response.Success)
                return BadRequest(response.Message);

            return Ok(response);
        }

        [HttpGet("{userId}/roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID cannot be null or empty.");

            var response = await _userService.GetUserRolesAsync(userId);

            if (!response.Success)
                return BadRequest(response.Message);

            return Ok(response);
        }
    }
}
