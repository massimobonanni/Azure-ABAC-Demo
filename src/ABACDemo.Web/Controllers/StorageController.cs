using ABACDemo.Web.Interfaces;
using ABACDemo.Web.Models.StorageController;
using Microsoft.AspNetCore.Mvc;

namespace ABACDemo.Web.Controllers
{
    public class StorageController : Controller
    {
        private readonly IContentsService _contentsService;
        private readonly ILogger<StorageController> _logger;

        public StorageController(IContentsService contentsService, ILogger<StorageController> logger)
        {
            ArgumentNullException.ThrowIfNull(contentsService);
            ArgumentNullException.ThrowIfNull(logger);

            _contentsService = contentsService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new IndexViewModel();
            try
            {
                model.Containers = await _contentsService.GetContainersAsync();
            }
            catch (Exception ex)
            {
                model.Exception = ex;
                model.Message = ex.Message;
            }
            return View(model);
        }
    }
}
