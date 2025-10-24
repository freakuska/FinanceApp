using Microsoft.AspNetCore.Mvc;

namespace FinanceApp.Web.Controllers;

public class AccountController : Controller
{
    /// <summary>
    /// Страница входа
    /// </summary>
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    /// <summary>
    /// Страница регистрации
    /// </summary>
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
}

