using FinanceApp.Infrastructure.Dtos;
using FinanceApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OperationsController(IFinancialOperationService operationService) : ControllerBase
{
    // GET /api/operations
    [HttpGet]
    public async Task<IActionResult> GetOperations([FromQuery] OperationFilterDto filter)
    {
        var userId = GetCurrentUserId(); // Из JWT токена
        var result = await operationService.GetPagedAsync(userId, filter);
        return Ok(result);
    }

    // GET /api/operations/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOperation(Guid id)
    {
        var userId = GetCurrentUserId();
        var operation = await operationService.GetByIdAsync(id, userId);
        
        if (operation == null)
            return NotFound();
        
        return Ok(operation);
    }

    // POST /api/operations
    [HttpPost]
    public async Task<IActionResult> CreateOperation([FromBody] CreateOperationDto dto)
    {
        var userId = GetCurrentUserId();
        var operation = await operationService.CreateAsync(dto, userId);
        
        return CreatedAtAction(
            nameof(GetOperation),
            new { id = operation.Id },
            operation);
    }

    // PUT /api/operations/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOperation(Guid id, [FromBody] UpdateOperationDto dto)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            var operation = await operationService.UpdateAsync(id, dto, userId);
            return Ok(operation);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // DELETE /api/operations/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOperation(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await operationService.DeleteAsync(id, userId);
        
        if (!result)
            return NotFound();
        
        return NoContent();
    }

    // POST /api/operations/{id}/restore
    [HttpPost("{id}/restore")]
    public async Task<IActionResult> RestoreOperation(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await operationService.RestoreAsync(id, userId);
        
        if (!result)
            return NotFound();
        
        return Ok();
    }

    // GET /api/operations/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = GetCurrentUserId();
        var stats = await operationService.GetStatsByCurrencyAsync(userId, startDate, endDate);
        
        return Ok(stats);
    }

    private Guid GetCurrentUserId()
    {
        // TODO: Получить из JWT токена
        // var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // return Guid.Parse(userIdClaim);
        
        return Guid.Parse("00000000-0000-0000-0000-000000000001"); // Заглушка
    }
}