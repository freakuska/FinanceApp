using FinanceApp.Api.Extensions;
using FinanceApp.Infrastructure.Dtos;
using FinanceApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Api.Controllers;

/// <summary>
/// Отчёты и аналитика
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Все методы требуют авторизации
public class ReportsController : ControllerBase
{
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // GET: api/report/monthly/{year}/{month}
        [HttpGet("monthly/{year}/{month}")]
        public async Task<IActionResult> GetMonthlyReport(int year, int month)
        {
            var userId = GetCurrentUserId(); // Из JWT токена
            var report = await _reportService.GetMonthlyReportAsync(userId, year, month);
            return Ok(report);
        }

        // GET: api/report/yearly/{year}
        [HttpGet("yearly/{year}")]
        public async Task<IActionResult> GetYearlyReport(int year)
        {
            var userId = GetCurrentUserId(); // Из JWT токена
            var report = await _reportService.GetYearlyReportAsync(userId, year);
            return Ok(report);
        }

        // GET: api/report/category
        [HttpGet("category")]
        public async Task<IActionResult> GetCategoryReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var userId = GetCurrentUserId(); // Из JWT токена
            var report = await _reportService.GetCategoryReportAsync(userId, startDate, endDate);
            return Ok(report);
        }

        // GET: api/report/trend
        [HttpGet("trend")]
        public async Task<IActionResult> GetTrendReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string groupBy = "day")
        {
            var userId = GetCurrentUserId(); // Из JWT токена
            var report = await _reportService.GetTrendReportAsync(userId, startDate, endDate, groupBy);
            return Ok(report);
        }

        // GET: api/report/export/csv
        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportToCsv(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var userId = GetCurrentUserId(); // Из JWT токена
            var csvData = await _reportService.ExportToCsvAsync(userId, startDate, endDate);
            return File(csvData, "text/csv", $"report_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}.csv");
        }

        // GET: api/report/export/excel
        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var userId = GetCurrentUserId(); // Из JWT токена
            var excelData = await _reportService.ExportToExcelAsync(userId, startDate, endDate);
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"report_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}.xlsx");
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue)
                throw new UnauthorizedAccessException("Пользователь не авторизован");
            
            return userId.Value;
        }
}