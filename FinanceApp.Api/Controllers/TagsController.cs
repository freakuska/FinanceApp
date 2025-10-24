using FinanceApp.Api.Extensions;
using FinanceApp.Dbo.Enums;
using FinanceApp.Infrastructure.Dtos;
using FinanceApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Api.Controllers;
/// <summary>
/// Управление тегами и категориями
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize] // Все методы требуют авторизации
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    /// <summary>
    /// Получить тег по ID
    /// </summary>
    /// <param name="id">ID тега</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tag = await _tagService.GetByIdAsync(id);
        
        if (tag == null)
            return NotFound(new { message = "Tag not found" });
        
        return Ok(tag);
    }

    /// <summary>
    /// Получить тег по slug
    /// </summary>
    /// <param name="slug">Slug тега (например: salary, groceries)</param>
    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var tag = await _tagService.GetBySlugAsync(slug);
        
        if (tag == null)
            return NotFound(new { message = $"Tag with slug '{slug}' not found" });
        
        return Ok(tag);
    }

    /// <summary>
    /// Получить теги по типу
    /// </summary>
    /// <param name="type">Тип тега (Income, Expense, Transfer)</param>
    /// <param name="userId">ID пользователя (опционально, для фильтрации приватных тегов)</param>
    [HttpGet("by-type/{type}")]
    [ProducesResponseType(typeof(List<TagDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByType(
        TagType type,
        [FromQuery] Guid? userId = null)
    {
        var tags = await _tagService.GetByTypeAsync(type, userId);
        return Ok(tags);
    }

    /// <summary>
    /// Получить дерево тегов
    /// </summary>
    /// <param name="type">Тип тега (опционально)</param>
    /// <param name="userId">ID пользователя (опционально)</param>
    [HttpGet("tree")]
    [ProducesResponseType(typeof(List<TagDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTree(
        [FromQuery] TagType? type = null,
        [FromQuery] Guid? userId = null)
    {
        var tree = await _tagService.GetTreeAsync(type, userId);
        return Ok(tree);
    }

    /// <summary>
    /// Получить популярные теги
    /// </summary>
    /// <param name="count">Количество тегов (по умолчанию 10)</param>
    [HttpGet("popular")]
    [ProducesResponseType(typeof(List<TagDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPopular([FromQuery] int count = 10)
    {
        if (count < 1 || count > 100)
            return BadRequest(new { message = "Count must be between 1 and 100" });
        
        var tags = await _tagService.GetPopularAsync(count);
        return Ok(tags);
    }

    /// <summary>
    /// Поиск тегов
    /// </summary>
    /// <param name="query">Поисковый запрос</param>
    /// <param name="userId">ID пользователя (опционально)</param>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<TagDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "Query parameter is required" });
        
        if (query.Length < 2)
            return BadRequest(new { message = "Query must be at least 2 characters" });
        
        var tags = await _tagService.SearchAsync(query, userId);
        return Ok(tags);
    }

    /// <summary>
    /// Создать новый тег
    /// </summary>
    /// <param name="dto">Данные нового тега</param>
    [HttpPost]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTagDto dto)
    {
        try
        {
            // В реальном приложении userId берётся из JWT токена
            var userId = GetCurrentUserId();
            
            var createDto = new CreateTagDto
            {
                Name = dto.Name,
                Type = dto.Type,
                ParentId = dto.ParentId,
                Icon = dto.Icon,
                Color = dto.Color,
                Visibility = dto.Visibility
            };

            var tag = await _tagService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Обновить тег
    /// </summary>
    /// <param name="id">ID тега</param>
    /// <param name="dto">Обновлённые данные</param>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTagDto dto)
    {
        try
        {
            var tag = await _tagService.UpdateAsync(id, dto);
            return Ok(tag);
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
    /// Удалить тег (мягкое удаление)
    /// </summary>
    /// <param name="id">ID тега</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _tagService.DeleteAsync(id);
            
            if (!result)
                return NotFound(new { message = "Tag not found or is system tag" });
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Изменить видимость тега
    /// </summary>
    /// <param name="id">ID тега</param>
    /// <param name="visibility">Новая видимость (Private/Public)</param>
    [HttpPatch("{id}/visibility")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeVisibility(
        Guid id,
        [FromBody] ChangeVisibilityRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _tagService.ChangeVisibilityAsync(id, request.Visibility, userId);
        
        if (!result)
            return NotFound(new { message = "Tag not found or you don't have permission" });
        
        return Ok(new { message = "Visibility updated successfully", visibility = request.Visibility });
    }

    /// <summary>
    /// Получить статистику по типам тегов
    /// </summary>
    /// <param name="userId">ID пользователя (опционально)</param>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats([FromQuery] Guid? userId = null)
    {
        // Эту функциональность нужно добавить в ITagService
        // Пока возвращаем заглушку
        return Ok(new Dictionary<string, int>
        {
            ["Income"] = 0,
            ["Expense"] = 0,
            ["Transfer"] = 0
        });
    }

    /// <summary>
    /// Получить все теги (без дерева)
    /// </summary>
    /// <param name="type">Фильтр по типу</param>
    /// <param name="visibility">Фильтр по видимости</param>
    /// <param name="userId">ID пользователя</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<TagDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] TagType? type = null,
        [FromQuery] TagVisibility? visibility = null,
        [FromQuery] Guid? userId = null)
    {
        if (type.HasValue)
        {
            var tags = await _tagService.GetByTypeAsync(type.Value, userId);
            
            // Фильтруем по видимости если указана
            if (visibility.HasValue)
            {
                tags = tags.Where(t => t.Visibility == visibility.Value.ToString()).ToList();
            }
            
            return Ok(tags);
        }

        // Если тип не указан, возвращаем дерево
        var tree = await _tagService.GetTreeAsync(null, userId);
        return Ok(tree);
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
            throw new UnauthorizedAccessException("Пользователь не авторизован");
        
        return userId.Value;
    }
    
    /// <summary>
    /// Запрос на изменение видимости тега
    /// </summary>
    public class ChangeVisibilityRequest
    {
        public TagVisibility Visibility { get; set; }
    }
}