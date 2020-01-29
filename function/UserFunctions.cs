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
using Surfrider.PlasticOrigins.Backend.Mobile.Service.Auth;
using Surfrider.PlasticOrigins.Backend.Mobile.ViewModel;

namespace Surfrider.PlasticOrigins.Backend.Mobile
{
    public class UserFunctions
    {
        private IUserService _userService;
        private IConfigurationService _configurationService;

        public UserFunctions(IUserService userService, IConfigurationService configurationService)
        {
            _userService = userService;
            _configurationService = configurationService;
        }

        [FunctionName("Register")]
        public async Task<IActionResult> RunRegister(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "register")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Register request");

            var registerVm = JsonConvert.DeserializeObject<RegisterViewModel>(await req.ReadAsStringAsync());

            var result = await _userService.Register(
                registerVm.LastName, 
                registerVm.FirstName, 
                registerVm.BirthYear,
                registerVm.Email, 
                registerVm.Password
                );
            
            
            return (ActionResult)new OkObjectResult(
                new
                {
                    Id = result.Id,
                    Token = result.AuthToken,
                    Expires = result.AuthTokenExpiration
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
            var token = await _userService.GenerateTokenFromPassword(loginRequest.Email, loginRequest.Password);

            return (ActionResult)new OkObjectResult(
                new
                {
                    Logged = true,
                    Token = token,
                    Expires = validityDate
                });
        }

        [FunctionName("RefreshToken")]
        public async Task<IActionResult> RunRefreshToken(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/refreshtoken")]
            HttpRequest req,
            [AccessToken] AccessTokenResult accessTokenResult,
            ILogger log)
        {
            log.LogInformation("#auth #refreshtoken");

            if (accessTokenResult.Status != AccessTokenStatus.Valid)
                return new UnauthorizedResult();

            JwtTokenContent rawToken = await AccessTokenValueProvider.GetRawToken(req, _configurationService.GetValue(ConfigurationServiceWellKnownKeys.JwtTokenSignatureKey));

            var newToken = await _userService.RefreshToken(rawToken);
            return (ActionResult)new OkObjectResult(
                new
                {
                    Token = newToken
                });
        }


        [FunctionName("UpdatePassword")]
        public async Task<IActionResult> RunUpdatePassword(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/updatepassword")]
            HttpRequest req,
            [AccessToken] AccessTokenResult accessTokenResult,
            ILogger log)
        {
            log.LogInformation("#auth #updatepassword");

            if (accessTokenResult.Status != AccessTokenStatus.Valid)
                return new UnauthorizedResult();


            var updatePasswordViewModel = JsonConvert.DeserializeObject<UpdatePasswordViewModel>(await req.ReadAsStringAsync());

            JwtTokenContent rawToken = await AccessTokenValueProvider.GetRawToken(req, _configurationService.GetValue(ConfigurationServiceWellKnownKeys.JwtTokenSignatureKey));

            await _userService.UpdatePassword(rawToken, updatePasswordViewModel.Password);

            return (ActionResult)new OkObjectResult(
                new
                {
                    Success = true
                });
        }

        [FunctionName("ResetAccount")]
        public async Task<IActionResult> RunResetAccount(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/reset")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"#auth #reset");
            // Todo: implement
            
            return (ActionResult)new OkObjectResult(
                new
                {
                    Success = true
                });
        }

    }
}
