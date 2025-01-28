using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Nettside.Data;
using Microsoft.AspNetCore.Authorization;
using Nettside.Repositiories;
using Nettside.ViewModels;
using Nettside.Models;

namespace Nettside.Controllers
{
    /// <summary>
    /// Controller responsible for managing actions related to the homepage, including displaying the main page, 
    /// handling error management, and processing area change registrations.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger; // Logger for tracking information and errors
        private readonly IGeoChangesRepository _geoChangesRepository; // Repository for managing GeoChanges
        private readonly IAreaChangeRepository _areaChangeRepository; // Repository for managing AreaChanges


        /// <summary>
        /// Constructor for injecting dependencies like the logger and repositories.
        /// </summary>
        public HomeController(
            ILogger<HomeController> logger,
            IGeoChangesRepository geoChangesRepository,
            IAreaChangeRepository areaChangeRepository)
        {
            _logger = logger;
            _geoChangesRepository = geoChangesRepository;
            _areaChangeRepository = areaChangeRepository;
        }


        /// <summary>
        /// Displays the homepage of the application.
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Displays a form for registering a new area change.
        /// </summary>
        [Authorize(Roles = "Caseworker, PrivateUser")]
        [HttpGet]
        public IActionResult RegisterAreaChange()
        {
            return View();
        }
        
        /// <summary>
        /// Handles the submission of data for a new area change.
        /// </summary>
        /// <param name="geoJson">GeoJSON data representing the area.</param>
        /// <param name="description">Description of the change.</param>
        /// <returns>A redirect to the map overview or an error message.</returns>
        [Authorize(Roles = "Caseworker, PrivateUser")]
        [HttpPost]
        public async Task<IActionResult> RegisterAreaChange(string geoJson, string description)
        {
            try
            {
                if (string.IsNullOrEmpty(geoJson) || string.IsNullOrEmpty(description))
                {
                    return BadRequest("Invalid data.");
                }

                var newGeoChange = new GeoChangesModel
                {
                    GeoJson = geoJson,
                    Description = description
                };

                await _geoChangesRepository.AddAsync(newGeoChange);

                return RedirectToAction("Index", "MapReport");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error registering area change: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Displays an overview of registered area and geo changes.
        /// </summary>
        /// <returns>A view with a list of changes.</returns>
        [Authorize(Roles = "Caseworker, PrivateUser")]
        [HttpGet]
        public async Task<IActionResult> AreaChangeOverview()
        {
            var geoChanges = await _geoChangesRepository.GetAllAsync();
            var areaChanges = await _areaChangeRepository.GetAllAsync();

            var viewModel = geoChanges.Select(change => new MapReportViewModel
            {
                GeoChanges = new List<GeoChangesModel> { change },
                AreaChanges = areaChanges
            }).ToList();

            return View(viewModel);
        }

        /// <summary>
        /// Displays a page to edit a spesific geographic change.
        /// </summary>
        /// <param name="id">The ID of the geographic change to be edited.</param>
        /// <returns>An edit view or a 404 error if the change is not found.</returns>
        [Authorize(Roles = "Caseworker")]
        [HttpGet]
        public async Task<IActionResult> EditGeoChangeView(int id)
        {
            var geoChange = await _geoChangesRepository.GetAsync(id);
            if (geoChange == null)
            {
                return NotFound($"GeoChange with ID {id} not found.");
            }

            return View(geoChange);
        }


        /// <summary>
        /// Updates a GeoChange in the database.
        /// </summary>
        /// <param name="geoChanges">The updated GeoChange model</param>
        /// <returns>A redirect to the overview page or an error message.</returns>
        [Authorize(Roles = "Caseworker")]
        [HttpPost]
        /* public async Task<IActionResult> EditGeoChange(GeoChangesModel geoChanges)
        {
            if (!ModelState.IsValid)
            {
                return View(geoChanges);
            }

            var result = await _geoChangesRepository.UpdateAsync(geoChanges);
            if (result == null)
            {
                return NotFound($"GeoChange with ID {geoChanges.Id} not found.");
            }

            return RedirectToAction("AreaChangeOverview");
        }

        /// <summary>
        /// Displays a confirmation page to delete a GeoChange.
        /// </summary>
        /// <param name="id">The ID of the GeoChange to be deleted.</param>
        /// <returns>A delete confirmation view or a 404 error if the change is not found.</returns>
        [Authorize(Roles = "Caseworker")]
        [HttpGet]
        public async Task<IActionResult> DeleteGeoChange(int id)
        {
            var geoChange = await _geoChangesRepository.GetAsync(id);
            if (geoChange == null)
            {
                return NotFound($"GeoChange with ID {id} not found.");
            }

            return View(geoChange);
        }

        /// <summary>
        /// Deletes a GeoChange from the database.
        /// </summary>
        /// <param name="id">The ID of the GeoChange to be deleted.</param>
        /// <returns>A redirect to the overview page or an error message.</returns>
        [Authorize(Roles = "Caseworker")]
        [HttpPost, ActionName("DeleteGeoChange")]
        public async Task<IActionResult> DeleteGeoChangeConfirmed(int id)
        {
            try
            {
                var result = await _geoChangesRepository.DeleteAsync(id);
                if (result == null)
                {
                    return NotFound($"GeoChange with ID {id} not found.");
                }

                return RedirectToAction("AreaChangeOverview");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting GeoChange: {ex.Message}");
                return BadRequest("An error occurred while deleting the GeoChange.");
            }
        }

        /// <summary>
        /// Displays the privacy policy page.
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
