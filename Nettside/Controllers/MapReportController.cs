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
        private readonly IGeoChangesRepository _geoChangesRepository;

        public MapReportController(
            ILogger<MapReportController> logger,
            IAreaChangeRepository areaChangeRepository,
            IGeoChangesRepository geoChangesRepository)
        {
            _logger = logger;
            _areaChangeRepository = areaChangeRepository;
            _geoChangesRepository = geoChangesRepository;
        }

        [Authorize(Roles = "Caseworker, PrivateUser")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var areaChanges = await _areaChangeRepository.GetAllAsync();
                var geoChanges = await _geoChangesRepository.GetAllAsync();

                var viewModel = new MapReportViewModel
                {
                    AreaChanges = areaChanges,
                    GeoChanges = geoChanges
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
    



