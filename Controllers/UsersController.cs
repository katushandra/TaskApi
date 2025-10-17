using Microsoft.AspNetCore.Mvc;

namespace TaskApi.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
