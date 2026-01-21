using ABACDemo.Web.Interfaces;
using ABACDemo.Web.Models.StorageController;
using Microsoft.AspNetCore.Mvc;

namespace ABACDemo.Web.Controllers
{
    public class StorageController : Controller
    {
        private readonly IContentsService _contentsService;
        private readonly ILogger<StorageController> _logger;
        private readonly IConfiguration _configuration;

        public StorageController(IContentsService contentsService, IConfiguration configuration, ILogger<StorageController> logger)
        {
            ArgumentNullException.ThrowIfNull(contentsService);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(logger);

            _contentsService = contentsService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new IndexViewModel();
            model.AccountName = _configuration["StorageAccountName"];
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

        public async Task<ActionResult> Container(string containerName)
        {
            var model = new ContainerViewModel();
            model.AccountName = _configuration["StorageAccountName"];
            model.ContainerName = containerName;

            try
            {
                model.Blobs = await this._contentsService.GetBlobsAsync(model.ContainerName, model.Date);
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
