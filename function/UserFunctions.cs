using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Surfrider.PlasticOrigins.Backend.Mobile.Service;
using Surfrider.PlasticOrigins.Backend.Mobile.ViewModel;

namespace Surfrider.PlasticOrigins.Backend.Mobile
{
    public class UserFunctions
    {
        private IUserService _userService;

        public UserFunctions(IUserService userService)
        {
            _userService = userService;
        }

        [FunctionName("Register")]
        public async Task<IActionResult> RunRegister(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "register")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Register request");

            var registerVM = JsonConvert.DeserializeObject<RegisterViewModel>(await req.ReadAsStringAsync());

            _userService.Register(registerVM.LastName, registerVM.FirstName, registerVM.BirthYear, registerVM.Email, registerVM.Password)
            
            return (ActionResult)new OkObjectResult(
                new
                {
                    Id = "qsjdqisjdjhgfquih"
                });
        }

        [FunctionName("Login")]
        public async Task<IActionResult> RunLogin(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "login")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"Login request");

            var loginRequest = JsonConvert.DeserializeObject<LoginRequestViewModel>(await req.ReadAsStringAsync());
            log.LogInformation($"Checking login for user {loginRequest.Email}");

            bool result = await _userService.CheckUserCredentials(loginRequest.Email, loginRequest.Password);

            if (result == false)
                return new UnauthorizedResult();

            var validityDate = DateTime.Now.AddHours(2048);

            var token = _userService.GenerateTokenFromPassword(loginRequest.Email, loginRequest.Password, validityDate);

            return (ActionResult)new OkObjectResult(
                new
                {
                    Logged = true,
                    Token = token,
                    Expires = validityDate
                });
        }
    }
}
