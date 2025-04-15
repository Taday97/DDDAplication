using DDDAplication.Application.DTOs;
using DDDAplication.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = $"User with ID {id} not found." });
            }
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserDto userUpdateDto)
        {
            if (userUpdateDto == null)
            {
                return BadRequest(new { Message = "User data is required." });
            }

            if (id != userUpdateDto.Id) // Ensure the ID matches
            {
                return BadRequest(new { Message = "The ID in the URL does not match the ID in the request body." });
            }

            var existingUser = await _userService.GetByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound(new { Message = $"User with ID {id} not found." });
            }

            var updatedUser = await _userService.Update(userUpdateDto);
            return Ok(updatedUser);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var existingUser = await _userService.GetByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound(new { Message = $"User with ID {id} not found." });
            }

            await _userService.Delete(id);
            return NoContent(); 
        }
    }
}
