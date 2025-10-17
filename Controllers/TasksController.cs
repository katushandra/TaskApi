using Microsoft.AspNetCore.Mvc;

namespace TaskApi.Controllers
{
    public class TasksController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
