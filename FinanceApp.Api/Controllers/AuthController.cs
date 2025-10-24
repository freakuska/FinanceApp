using FinanceApp.Infrastructure.Dtos;
using FinanceApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Api.Controllers;

/// <summary>
/// Контроллер аутентификации и авторизации
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    /// <param name="dto">Данные для регистрации</param>
    /// <returns>JWT токены и данные пользователя</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        try
        {
            var response = await _authService.RegisterAsync(dto);
            return CreatedAtAction(nameof(GetMe), null, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Вход в систему
    /// </summary>
    /// <param name="dto">Email и пароль</param>
    /// <returns>JWT токены и данные пользователя</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        try
        {
            var response = await _authService.LoginAsync(dto);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Обновление access токена с помощью refresh токена
    /// </summary>
    /// <param name="dto">Refresh токен</param>
    /// <returns>Новые JWT токены</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(dto.RefreshToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Выход из системы (отзыв refresh токена)
    /// </summary>
    /// <param name="dto">Refresh токен для отзыва</param>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        var result = await _authService.RevokeTokenAsync(dto.RefreshToken);
        return result 
            ? Ok(new { message = "Успешный выход из системы" }) 
            : BadRequest(new { message = "Недействительный токен" });
    }

    /// <summary>
    /// Получение данных текущего авторизованного пользователя
    /// </summary>
    /// <returns>Данные пользователя</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Недействительный токен" });

        var user = await _userService.GetByIdAsync(userId);
        
        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

        return Ok(user);
    }
}

