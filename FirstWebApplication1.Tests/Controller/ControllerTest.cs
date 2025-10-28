using FirstWebApplication1.Controllers;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace FirstWebApplication1.Tests.Controller
{
    public class ControllerTest
    {
        // Lager en testkonfigurasjon i minnet (uten database)
        private HomeController CreateController()
        {
            var settings = new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Server=localhost;Database=test;User=test;Password=test;" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            return new HomeController(configuration);
        }

        // Tester at controlleren kan opprettes uten feil
        [Fact]
        public void HomeController_CanBeCreated()
        {
            var controller = CreateController();
            Assert.NotNull(controller);
        }

        // Tester at Index()-metoden kjører uten unntak
        [Fact]
        public void Index_DoesNotThrow()
        {
            var controller = CreateController();

            var exception = Record.Exception(() => controller.Index());

            Assert.Null(exception);
        }

        // Tester at Privacy()-metoden kjører uten unntak
        [Fact]
        public void Privacy_DoesNotThrow()
        {
            var controller = CreateController();

            var exception = Record.Exception(() => controller.Privacy());

            Assert.Null(exception);
        }

        // Tester at Error()-metoden kjører uten unntak
        [Fact]
        public void Error_DoesNotThrow()
        {
            var controller = CreateController();

            var exception = Record.Exception(() => controller.Error());

            Assert.Null(exception);
        }
    }
}
