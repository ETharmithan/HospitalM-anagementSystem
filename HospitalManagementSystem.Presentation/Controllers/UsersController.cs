using HospitalManagementSystem.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = users.Select(u => new UserDto
            {
                UserId = u.UserId.ToString(),
                Email = u.Email,
                Username = u.Username,
                Role = u.Role,
                ImageUrl = u.ImageUrl,
                IsEmailVerified = u.IsEmailVerified
            });
            return Ok(userDtos);
        }

        // GET: api/users/role/{role}
        [HttpGet("role/{role}")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByRole(string role)
        {
            var users = await _userRepository.GetAllAsync();
            var filteredUsers = users.Where(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase));
            
            var userDtos = filteredUsers.Select(u => new UserDto
            {
                UserId = u.UserId.ToString(),
                Email = u.Email,
                Username = u.Username,
                Role = u.Role,
                ImageUrl = u.ImageUrl,
                IsEmailVerified = u.IsEmailVerified
            });
            return Ok(userDtos);
        }

        // GET: api/users/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserDto>> GetUserById(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new UserDto
            {
                UserId = user.UserId.ToString(),
                Email = user.Email,
                Username = user.Username,
                Role = user.Role,
                ImageUrl = user.ImageUrl,
                IsEmailVerified = user.IsEmailVerified
            });
        }

        // PUT: api/users/{id}/role
        [HttpPut("{id:guid}/role")]
        public async Task<ActionResult> UpdateUserRole(Guid id, [FromBody] UpdateRoleRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var validRoles = new[] { "SuperAdmin", "Admin", "Doctor", "Patient" };
            if (!validRoles.Contains(request.Role))
                return BadRequest(new { message = "Invalid role" });

            user.Role = request.Role;
            await _userRepository.SaveChangesAsync();

            return Ok(new { message = "User role updated successfully" });
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // Prevent deleting SuperAdmin
            if (user.Role == "SuperAdmin")
                return BadRequest(new { message = "Cannot delete SuperAdmin user" });

            await _userRepository.DeleteAsync(id);
            await _userRepository.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully" });
        }
    }

    public class UserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsEmailVerified { get; set; }
    }

    public class UpdateRoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }
}
