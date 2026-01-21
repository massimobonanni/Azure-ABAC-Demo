using ABACDemo.Web.Models;
using ABACDemo.Web.Models.HomeController;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using System.Diagnostics;

namespace ABACDemo.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration); 
            this._configuration = configuration;
        }


        public IActionResult Index()
        {
            var model= new IndexViewModel();
            model.AccountName = _configuration["StorageAccountName"];

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
