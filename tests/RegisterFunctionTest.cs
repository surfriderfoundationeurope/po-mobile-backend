using Microsoft.AspNetCore.Mvc;
using Surfrider.PlasticOrigins.Backend.Mobile.Service;
using Surfrider.PlasticOrigins.Backend.Mobile.Tests.Infrastructure;
using Xunit;
using Surfrider.PlasticOrigins.Backend.Mobile;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Tests
{
    public class RegisterFunctionTest : SurfriderBaseFunctionTest
    {
        [Fact]
        public async void Register_should_return_id()
        {
            var request = TestFactory.CreateHttpRequest(new
            {
                lastName = "Montmirail",
                firstName = "Eudes",
                birthYear = 1969,
                email = "eudes.montpirail@corp.com",
                password = "Compl1catedP@ssw0rd"
            });

            var userFunctionsHost = new UserFunctions(new UserService(new InMemoryUserStore()));
            
            var response = (OkObjectResult) await userFunctionsHost.RunRegister(request, Logger);
            
            Assert.Equal(200, response.StatusCode);

            // TODO: Should return token
            // TODO: Should return expiration 
        }
    }
}
