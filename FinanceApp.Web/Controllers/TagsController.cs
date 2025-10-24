using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Web.Controllers;

public class TagsController : Controller
{
    /// <summary>
    /// Страница управления тегами
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}

