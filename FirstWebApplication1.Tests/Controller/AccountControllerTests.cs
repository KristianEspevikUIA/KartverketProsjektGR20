using FirstWebApplication1.Controllers;
using FirstWebApplication1.Models.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

// Alias for å unngå konflikt mellom Identity.SignInResult og MVC.SignInResult
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace FirstWebApplication1.Tests.Controllers
{
    public class AccountControllerTests
    {
        // ------------------ HJELPEMETODER ------------------

        // Lager en enkel IConfiguration med Admin:Email
        private IConfiguration CreateConfiguration(string adminEmail = "admin@kartverket.no")
        {
            var settings = new Dictionary<string, string?>
            {
                { "Admin:Email", adminEmail }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        // Mock av UserManager
        private Mock<UserManager<IdentityUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            return new Mock<UserManager<IdentityUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        // Mock av RoleManager (vi bruker den ikke så mye, men controlleren trenger den)
        private Mock<RoleManager<IdentityRole>> CreateRoleManagerMock()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(
                store.Object, null, null, null, null);
        }

        // Mock av SignInManager
        private Mock<SignInManager<IdentityUser>> CreateSignInManagerMock(UserManager<IdentityUser> userManager)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            contextAccessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());

            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();

            return new Mock<SignInManager<IdentityUser>>(
                userManager,
                contextAccessor.Object,
                claimsFactory.Object,
                null, null, null, null);
        }

        // Lager en AccountController med alle mockede avhengigheter
        private AccountController CreateController(
            Mock<UserManager<IdentityUser>>? userManagerMock = null,
            Mock<SignInManager<IdentityUser>>? signInManagerMock = null,
            Mock<RoleManager<IdentityRole>>? roleManagerMock = null,
            IConfiguration? configuration = null)
        {
            var userManager = userManagerMock ?? CreateUserManagerMock();
            var signInManager = signInManagerMock ?? CreateSignInManagerMock(userManager.Object);
            var roleManager = roleManagerMock ?? CreateRoleManagerMock();
            configuration ??= CreateConfiguration();

            var controller = new AccountController(
                userManager.Object,
                signInManager.Object,
                roleManager.Object,
                configuration);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            return controller;
        }

        // ------------------ TEST 1 ------------------
        // Register (POST): Ønsker rollen "Admin" med ikke-godkjent e-post
        // => Skal gi ModelState-feil og IKKE opprette bruker
        [Fact]
        public async Task Register_AdminRoleWithUnauthorizedEmail_AddsModelErrorAndReturnsView()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
            var roleManagerMock = CreateRoleManagerMock();

            // Kun denne e-posten er lov som admin
            var config = CreateConfiguration(adminEmail: "admin@kartverket.no");

            var controller = CreateController(userManagerMock, signInManagerMock, roleManagerMock, config);

            var model = new RegisterViewModel
            {
                Email = "randomuser@example.com",
                Password = "Password123!",
                Role = "Admin"
            };

            // Act
            var result = await controller.Register(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(nameof(model.Role)));

            // Sjekk at CreateAsync aldri blir kalt (ingen bruker opprettes)
            userManagerMock.Verify(
                um => um.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()),
                Times.Never);
        }

        // ------------------ TEST 2 ------------------
        // Register (POST): Gyldig vanlig bruker (ikke admin)
        // => Skal opprette bruker, legge til rolle, logge inn og redirecte til Home/Index
        [Fact]
        public async Task Register_ValidNonAdminUser_CreatesUserAssignsRoleAndRedirectsHome()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
            var roleManagerMock = CreateRoleManagerMock();
            var config = CreateConfiguration();

            // Opprettelse av bruker lykkes
            userManagerMock
                .Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Legge til rolle lykkes
            userManagerMock
                .Setup(um => um.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var controller = CreateController(userManagerMock, signInManagerMock, roleManagerMock, config);

            var model = new RegisterViewModel
            {
                Email = "pilot@example.com",
                Password = "Password123!",
                Role = "Pilot"
            };

            // Act
            var result = await controller.Register(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);

            userManagerMock.Verify(
                um => um.CreateAsync(It.Is<IdentityUser>(u => u.Email == model.Email), model.Password),
                Times.Once);

            userManagerMock.Verify(
                um => um.AddToRoleAsync(It.IsAny<IdentityUser>(), "Pilot"),
                Times.Once);

            signInManagerMock.Verify(
                sm => sm.SignInAsync(It.IsAny<IdentityUser>(), false, null),
                Times.Once);
        }

        // ------------------ TEST 3 ------------------
        // Login (POST): Gyldige credentials
        // => Skal kalle PasswordSignInAsync og redirecte til Home/Index når returnUrl er null
        [Fact]
        public async Task Login_ValidCredentials_RedirectsToHomeWhenNoReturnUrl()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
            var roleManagerMock = CreateRoleManagerMock();
            var config = CreateConfiguration();

            // Simulerer vellykket innlogging
            signInManagerMock
                .Setup(sm => sm.PasswordSignInAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    false))
                .ReturnsAsync(IdentitySignInResult.Success);

            var controller = CreateController(userManagerMock, signInManagerMock, roleManagerMock, config);

            // Url-helper trengs fordi RedirectToLocal bruker Url.IsLocalUrl(...)
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.IsLocalUrl(It.IsAny<string>()))
                .Returns(false);
            controller.Url = urlHelperMock.Object;

            var model = new LoginViewModel
            {
                Email = "user@example.com",
                Password = "Password123!",
                RememberMe = false
            };

            // Act
            var result = await controller.Login(model, returnUrl: null);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);

            signInManagerMock.Verify(
                sm => sm.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false),
                Times.Once);
        }

        // ------------------ TEST 4 ------------------
        // Login (POST): Feil passord/bruker
        // => Skal legge til ModelState-feil og returnere View med samme model
        [Fact]
        public async Task Login_InvalidCredentials_AddsModelErrorAndReturnsView()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
            var roleManagerMock = CreateRoleManagerMock();
            var config = CreateConfiguration();

            signInManagerMock
                .Setup(sm => sm.PasswordSignInAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    false))
                .ReturnsAsync(IdentitySignInResult.Failed);

            var controller = CreateController(userManagerMock, signInManagerMock, roleManagerMock, config);

            var model = new LoginViewModel
            {
                Email = "user@example.com",
                Password = "wrong",
                RememberMe = false
            };

            // Act
            var result = await controller.Login(model, returnUrl: "/somewhere");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);

            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ContainsKey(string.Empty)); // general error key
        }

        // ------------------ TEST 5 ------------------
        // Logout (POST): Skal kalle SignOutAsync og redirecte til Home/Index
        [Fact]
        public async Task Logout_SignsOutAndRedirectsHome()
        {
            // Arrange
            var userManagerMock = CreateUserManagerMock();
            var signInManagerMock = CreateSignInManagerMock(userManagerMock.Object);
            var roleManagerMock = CreateRoleManagerMock();
            var config = CreateConfiguration();

            var controller = CreateController(userManagerMock, signInManagerMock, roleManagerMock, config);

            // Act
            var result = await controller.Logout();

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);

            signInManagerMock.Verify(sm => sm.SignOutAsync(), Times.Once);
        }
    }
}