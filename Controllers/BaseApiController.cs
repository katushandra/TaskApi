using Microsoft.AspNetCore.Mvc;

namespace TaskApi.Controllers
{
    public class BaseApiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
