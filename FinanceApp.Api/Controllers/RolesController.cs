using FinanceApp.Infrastructure.Dtos;
using FinanceApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Api.Controllers;

/// <summary>
/// Управление ролями пользователей
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// Получить все роли
    /// </summary>
    /// <returns>Список всех ролей в системе</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _roleService.GetAllAsync();
        return Ok(roles);
    }

    /// <summary>
    /// Получить роль по ID
    /// </summary>
    /// <param name="id">ID роли</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var role = await _roleService.GetByIdAsync(id);
        
        if (role == null)
            return NotFound(new { message = "Role not found" });
        
        return Ok(role);
    }

    /// <summary>
    /// Получить роль по коду
    /// </summary>
    /// <param name="code">Код роли (например: USER, ADMIN)</param>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code)
    {
        var role = await _roleService.GetByCodeAsync(code.ToUpper());
        
        if (role == null)
            return NotFound(new { message = $"Role with code '{code}' not found" });
        
        return Ok(role);
    }

    /// <summary>
    /// Создать новую роль
    /// </summary>
    /// <param name="dto">Данные новой роли</param>
    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        try
        {
            var role = await _roleService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Обновить роль
    /// </summary>
    /// <param name="id">ID роли</param>
    /// <param name="dto">Обновлённые данные</param>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleDto dto)
    {
        try
        {
            var role = await _roleService.UpdateAsync(id, dto);
            return Ok(role);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Удалить роль
    /// </summary>
    /// <param name="id">ID роли</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _roleService.DeleteAsync(id);
        
        if (!result)
            return BadRequest(new { message = "Cannot delete system role or role not found" });
        
        return NoContent();
    }

    /// <summary>
    /// Проверить наличие разрешения у пользователя
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="permission">Название разрешения</param>
    [HttpGet("users/{userId}/permissions/{permission}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> HasPermission(Guid userId, string permission)
    {
        var hasPermission = await _roleService.HasPermissionAsync(userId, permission);
        return Ok(new { hasPermission, permission, userId });
    }

    /// <summary>
    /// Получить все разрешения пользователя
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    [HttpGet("users/{userId}/permissions")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserPermissions(Guid userId)
    {
        var permissions = await _roleService.GetUserPermissionsAsync(userId);
        return Ok(new { userId, permissions });
    }
}