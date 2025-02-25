using Xunit;
using Moq;
using Nettside.Controllers;
using Nettside.Models;
using Nettside.Repositiories;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Nettside.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Nettside.UnitTestingMetode
{
    public class NettsideUnitTests
    {
        private readonly HomeController _controller;
        private readonly Mock<IAreaChangeRepository> _mockAreaChangeRepository;
        private readonly Mock<UserManager<Users>> _mockUserManager;

        public NettsideUnitTests()
        {
            _mockAreaChangeRepository = new Mock<IAreaChangeRepository>();

            _mockUserManager = new Mock<UserManager<Users>>(
                Mock.Of<IUserStore<Users>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<Users>>(),
                new List<IUserValidator<Users>> { Mock.Of<IUserValidator<Users>>() },
                new List<IPasswordValidator<Users>> { Mock.Of<IPasswordValidator<Users>>() },
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<Users>>>()
                );

            // Setup user manager mock to return a specific user (you can customize this for your tests)
            var mockUser = new Users { Id = "testUserId" };
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(mockUser);

            // Instantiate the controller with the mocked dependencies
            var mockLogger = new Mock<ILogger<HomeController>>();
            _controller = new HomeController(mockLogger.Object, _mockAreaChangeRepository.Object, _mockUserManager.Object);
        }

        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Test that the Index action returns a ViewResult
            var result = _controller.Index();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void RegisterAreaChange_ReturnsViewResult()
        {
            // Test that the RegisterAreaChange action returns a ViewResult
            var result = _controller.RegisterAreaChange();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task RegisterAreaChange_InvalidInput_ReturnsBadRequest()
        {
            // Test that the RegisterAreaChange action returns a BadRequestObjectResult when the input is invalid
            AreaChangesViewModel viewModel = null;
            var result = await _controller.RegisterAreaChange(viewModel);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task AreaChangeOverview_ReturnsViewResult_WithAreaChanges()
        {
            // Test that the AreaChangeOverview action returns a ViewResult with a list of AreaChangeModel
            var areaChanges = new List<AreaChangeModel>
            {
                new AreaChangeModel { UserName = "User1", Kommunenavn = "Oslo", Fylkenavn = "Oslo", Description = "Change 1", AreaJson = "{}" },
                new AreaChangeModel { UserName = "User2", Kommunenavn = "Bergen", Fylkenavn = "Vestland", Description = "Change 2", AreaJson = "{}" }
            };

            _mockAreaChangeRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(areaChanges);
            var result = await _controller.AreaChangeOverview();
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<AreaChangeModel>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task EditAreaChangeView_ValidId_ReturnsViewResultWithModel()
        {
            // Test that the EditAreaChangeView action returns a ViewResult with the correct AreaChangesViewModel when a valid ID is provided
            var areaChange = new AreaChangeModel
            {
                Kommunenavn = "Oslo",
                Fylkenavn = "Oslo",
                Description = "Test Description",
                AreaJson = "{}"
            };
            _mockAreaChangeRepository.Setup(repo => repo.FindCaseById(It.IsAny<int>())).ReturnsAsync(areaChange);
            var result = await _controller.EditAreaChangeView(1);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AreaChangesViewModel>(viewResult.Model);
            Assert.Equal("Oslo", model.ViewKommunenavn);
            Assert.Equal("Oslo", model.ViewFylkenavn);
            Assert.Equal("Test Description", model.ViewDescription);
            Assert.Equal("{}", model.ViewAreaJson);
        }

        [Fact]
        public async Task EditAreaChangeView_InvalidId_ReturnsNull()
        {
            // Test that the EditAreaChangeView action returns null when an invalid ID is provided
            _mockAreaChangeRepository.Setup(repo => repo.FindCaseById(It.IsAny<int>())).ReturnsAsync((AreaChangeModel)null);
            var result = await _controller.EditAreaChangeView(1);
            Assert.Null(result);
        }

        [Fact]
        public async Task EditAreaChange_ValidInput_RedirectsToAreaChangeOverview()
        {
            // Test that the EditAreaChange action redirects to AreaChangeOverview when valid input is provided
            var viewModel = new AreaChangesViewModel
            {
                Id = 1,
                ViewKommunenavn = "Oslo",
                ViewFylkenavn = "Oslo",
                ViewDescription = "Updated Description",
                ViewAreaJson = "{}"
            };

            var existingAreaChange = new AreaChangeModel
            {
                Id = 1,
                Kommunenavn = "Bergen",
                Fylkenavn = "Vestland",
                Description = "Original Description",
                AreaJson = "{}"
            };

            _mockAreaChangeRepository.Setup(repo => repo.FindCaseById(viewModel.Id)).ReturnsAsync(existingAreaChange);
            _mockAreaChangeRepository.Setup(repo => repo.UpdateAsync(It.IsAny<AreaChangeModel>())).ReturnsAsync(existingAreaChange);
            var result = await _controller.EditAreaChange(viewModel);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AreaChangeOverview", redirectResult.ActionName);
        }

        [Fact]
        public async Task EditAreaChange_InvalidInput_ReturnsNull()
        {
            // Test that the EditAreaChange action returns null when invalid input is provided
            var viewModel = new AreaChangesViewModel { Id = 1 };
            _mockAreaChangeRepository.Setup(repo => repo.FindCaseById(viewModel.Id)).ReturnsAsync((AreaChangeModel)null);
            var result = await _controller.EditAreaChange(viewModel);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAreaChange_AreaChangeFound_RedirectsToAreaChangeOverview()
        {
            // Test that the DeleteAreaChange action redirects to AreaChangeOverview when the area change is found
            int areaChangeId = 1;
            var areaChange = new AreaChangeModel { Id = areaChangeId };
            _mockAreaChangeRepository.Setup(repo => repo.FindCaseById(areaChangeId)).ReturnsAsync(areaChange);
            _mockAreaChangeRepository.Setup(repo => repo.DeleteAsync(areaChangeId)).ReturnsAsync(areaChange);
            var result = await _controller.DeleteAreaChange(areaChangeId);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AreaChangeOverview", redirectToActionResult.ActionName);
            Assert.Equal("Home", redirectToActionResult.ControllerName);
        }

        [Fact]
        public async Task DeleteAreaChange_AreaChangeNotFound_ReturnsNotFoundResult()
        {
            // Test that the DeleteAreaChange action returns a NotFoundObjectResult when the area change is not found
            int areaChangeId = 1;
            _mockAreaChangeRepository.Setup(repo => repo.FindCaseById(areaChangeId)).ReturnsAsync((AreaChangeModel)null);
            var result = await _controller.DeleteAreaChange(areaChangeId);
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}