using FinanceApp.Api.Extensions;
using FinanceApp.Infrastructure.Dtos;
using FinanceApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Api.Controllers;

/// <summary>
/// Управление пользователями
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Базовая авторизация для всех методов
public class UsersController : ControllerBase
{
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "SUPER_ADMIN,ADMIN,MANAGER")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userService.GetByIdAsync(id);
            return user == null ? NotFound() : Ok(user);
        }

        // GET: api/user/email/{email}
        [HttpGet("email/{email}")]
        [Authorize(Roles = "SUPER_ADMIN,ADMIN")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var user = await _userService.GetByEmailAsync(email);
            return user == null ? NotFound() : Ok(user);
        }

        // POST: api/user
        [HttpPost]
        [Authorize(Roles = "SUPER_ADMIN,ADMIN")]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            try
            {
                var createdUser = await _userService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/user/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var updatedUser = await _userService.UpdateAsync(id, dto);
                return Ok(updatedUser);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "SUPER_ADMIN,ADMIN")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _userService.DeleteAsync(id);
            return result ? NoContent() : NotFound();
        }

        // POST: api/user/change-password
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _userService.ChangePasswordAsync(request.UserId, new ChangePasswordDto
            {
                CurrentPassword = request.CurrentPassword,
                NewPassword = request.NewPassword
            });
            return result ? Ok() : BadRequest("Invalid current password");
        }

        // POST: api/user/verify-email
        [HttpPost("verify-email/{id}")]
        public async Task<IActionResult> VerifyEmail(Guid id)
        {
            var result = await _userService.VerifyEmailAsync(id);
            return result ? Ok() : NotFound();
        }

        // GET: api/user/all
        [HttpGet("all")]
        [Authorize(Roles = "SUPER_ADMIN,ADMIN,MANAGER")]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 50)
        {
            var users = await _userService.GetAllAsync(page, pageSize);
            return Ok(users);
        }

        // POST: api/user/assign-role
        [HttpPost("assign-role")]
        [Authorize(Roles = "SUPER_ADMIN,ADMIN")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            var result = await _userService.AssignRoleAsync(request.UserId, request.RoleCode, request.AssignedBy);
            return result ? Ok() : BadRequest("Failed to assign role");
        }

        // POST: api/user/remove-role
        [HttpPost("remove-role")]
        [Authorize(Roles = "SUPER_ADMIN,ADMIN")]
        public async Task<IActionResult> RemoveRole([FromBody] RemoveRoleRequest request)
        {
            var result = await _userService.RemoveRoleAsync(request.UserId, request.RoleCode);
            return result ? Ok() : BadRequest("Failed to remove role");
        }

        // GET: api/user/roles/{userId}
        [HttpGet("roles/{userId}")]
        public async Task<IActionResult> GetUserRoles(Guid userId)
        {
            var roles = await _userService.GetUserRolesAsync(userId);
            return Ok(roles);
        }

        // POST: api/user/update-last-login
        [HttpPost("update-last-login/{userId}")]
        public async Task<IActionResult> UpdateLastLogin(Guid userId)
        {
            await _userService.UpdateLastLoginAsync(userId);
            return Ok();
        }
    }

    public class ChangePasswordRequest
    {
        public Guid UserId { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class AssignRoleRequest
    {
        public Guid UserId { get; set; }
        public string RoleCode { get; set; }
        public Guid AssignedBy { get; set; }
    }

    public class RemoveRoleRequest
    {
        public Guid UserId { get; set; }
        public string RoleCode { get; set; }
    }
    