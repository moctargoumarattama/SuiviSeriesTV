using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SuiviSeriesTV.Controllers;

[Authorize]
public class UserController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction("Dashboard", "Series");
    }
}
