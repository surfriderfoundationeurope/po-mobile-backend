using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Surfrider.PlasticOrigins.Backend.Mobile.Tests.Infrastructure;
using Surfrider.PlasticOrigins.Backend.Mobile;
using Xunit;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Tests
{
    public class UnitTest1
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void Test1()
        {
            var request = TestFactory.CreateHttpRequest("name", "Bill");
            var response = (OkObjectResult)await Heartbeat.Run(request, logger);
            Assert.Equal(200, response.StatusCode);
        }
    }
}
