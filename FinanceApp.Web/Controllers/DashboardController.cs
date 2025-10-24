using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Web.Controllers;

public class DashboardController : Controller
{
    /// <summary>
    /// Главная страница дашборда
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}

