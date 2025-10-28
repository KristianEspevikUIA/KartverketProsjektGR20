using FirstWebApplication1.Controllers;
using FirstWebApplication1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstWebApplication1.Tests.Views
{
    public class ViewTest

    {
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
        // Tester at Index()-metoden returnerer en ViewResult 
        [Fact]
        public void Index_ReturnsViewResult()
        {
            var controller = CreateController();

            var result = controller.Index();

            Assert.IsType<ViewResult>(result);
        }

        // Tester at Privacy()-metoden returnerer en ViewResult 
        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            var controller = CreateController();

            var result = controller.Privacy();

            Assert.IsType<ViewResult>(result);
        }

        // Tester at Error()-metoden returnerer en ViewResult 
        [Fact]
        public void Error_ReturnsViewWithErrorViewModel()
        {
            var controller = CreateController();

            // Legger til en kunstig HttpContext slik at TraceIdentifier ikke er null
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = controller.Error();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ErrorViewModel>(viewResult.Model);
            Assert.False(string.IsNullOrEmpty(model.RequestId));
        }
    }
}

