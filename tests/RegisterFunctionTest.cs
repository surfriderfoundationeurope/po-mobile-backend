using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Surfrider.PlasticOrigins.Backend.Mobile.Tests.Infrastructure;
using Xunit;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Tests
{
    public class RegisterFunctionTest : SurfriderBaseFunctionTest
    {
        [Fact]
        public async void Test1()
        {
            var request = TestFactory.CreateHttpRequest("name", "Bill");
            var response = (OkObjectResult)await Heartbeat.Run(request, Logger);
            Assert.Equal(200, response.StatusCode);
        }
    }
}
