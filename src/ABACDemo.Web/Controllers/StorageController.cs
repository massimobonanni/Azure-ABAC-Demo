using Microsoft.AspNetCore.Mvc;

namespace ABACDemo.Web.Controllers
{
    public class StorageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
