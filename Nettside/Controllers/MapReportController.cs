using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nettside.Repositiories;
using Nettside.ViewModels;

namespace Nettside.Controllers
{
    public class MapReportController : Controller
    {
        private readonly ILogger<MapReportController> _logger;
        private readonly IAreaChangeRepository _areaChangeRepository;
       

        public MapReportController(
            ILogger<MapReportController> logger,
            IAreaChangeRepository areaChangeRepository)
        {
            _logger = logger;
            _areaChangeRepository = areaChangeRepository;
            
        }

        [Authorize(Roles = "Caseworker, PrivateUser")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var areaChanges = await _areaChangeRepository.GetAllAsync();
                
                var viewModel = new MapReportViewModel
                {
                    AreaChanges = areaChanges,
                   
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving map reports: {ex.Message}");
                return View("Error");
            }
        }



    }



}
    



