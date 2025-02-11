using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Nettside.Data;
using Microsoft.AspNetCore.Authorization;
using Nettside.Repositiories;
using Nettside.ViewModels;
using Nettside.Models;
using Nettside.Models.ViewModel;
using Microsoft.AspNetCore.Identity;

namespace Nettside.Controllers
{
    /// <summary>
    /// Controller responsible for managing actions related to the homepage, including displaying the main page, 
    /// handling error management, and processing area change registrations.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger; // Logger for tracking information and errors

        private readonly IAreaChangeRepository _areaChangeRepository; // Repository for managing AreaChanges

        private readonly UserManager<Users> _userManager;


        /// <summary>
        /// Constructor for injecting dependencies like the logger and repositories.
        /// </summary>
        public HomeController(
            ILogger<HomeController> logger,

            IAreaChangeRepository areaChangeRepository, UserManager<Users> userManager)
        {
            _logger = logger;

            _areaChangeRepository = areaChangeRepository;

            _userManager = userManager;
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
        /// <param name="areaJson"></param>
        /// <param name="description">Description of the change.</param>
        /// <returns>A redirect to the map overview or an error message.</returns>
        [Authorize(Roles = "Caseworker, PrivateUser")]
        [HttpPost]
        public async Task<IActionResult> RegisterAreaChange(AreaChangesViewModel areaChangesViewModel)
        {

            var currentUser = await _userManager.GetUserAsync(User);


            if (areaChangesViewModel != null && currentUser != null)
            {
                var newAreaChange = new AreaChangeModel
                {
                    Id = Guid.NewGuid(),
                    UserName = currentUser.UserName,
                    Kommunenavn = areaChangesViewModel.ViewKommunenavn,
                    Fylkenavn = areaChangesViewModel.ViewFylkenavn,
                    Description = areaChangesViewModel.ViewDescription,
                    AreaJson = areaChangesViewModel.ViewAreaJson
                };

                await _areaChangeRepository.AddAsync(newAreaChange);

                return RedirectToAction("AreaChangeOverview", "Home");

            }

            return BadRequest("An error occured");
           
        }



        /// <summary>
        /// Displays an overview of registered area and geo changes.
        /// </summary>
        /// <returns>A view with a list of changes.</returns>
        [Authorize(Roles = "Caseworker, PrivateUser")]
        [HttpGet]
        public async Task<IActionResult> AreaChangeOverview()
        {

            var areaChanges = await _areaChangeRepository.GetAllAsync();    

            return View(areaChanges);
        }


        /// <summary>
        /// Displays a page to edit a spesific geographic change.
        /// </summary>
        /// <param name="id">The ID of the geographic change to be edited.</param>
        /// <returns>An edit view or a 404 error if the change is not found.</returns>
        [Authorize(Roles = "Caseworker")]
        [HttpGet]
        public async Task<IActionResult> EditAreaChangeView(Guid id)
        {
            var areaChange = await _areaChangeRepository.GetAsync(id);
            if (areaChange == null)
            {
                return NotFound($"AreaChange with ID {id} not found.");
            }

            return View(areaChange);
        }


        [Authorize(Roles = "Caseworker")]
        [HttpPost]
        public async Task<IActionResult> DeleteAreaChange(Guid id)
        {
            var areaChange = await _areaChangeRepository.GetAsync(id);
            if (areaChange == null)
            {
                return NotFound($"AreaChange with ID {id} not found.");
            }

            await _areaChangeRepository.DeleteAsync(id);
            return RedirectToAction("AreaChangeOverview", "Home");
        }
    }
}


       