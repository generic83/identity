using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using WeatherMVC.Models;
using WeatherMVC.Services;

namespace WeatherMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITokenService _tokenService;

        public HomeController(ILogger<HomeController> logger, ITokenService tokenService)
        {
            _logger = logger;
            _tokenService = tokenService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Roles ="admin")]
        public async Task<IActionResult> Weather()
        {
            var data = new List<WeatherData>();
            //access_token
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            //id_token
            //var idtoken = await HttpContext.GetTokenAsync("id_token");

            //refresh token
            //AccessToken and new refresh token should be stored for next use
            //The code below works fine if RefreshTokenUsage set to TokenUsage.ReUse in IdentityServer client configuration
            //as the same refresh token can be used
            //var handler = new JwtSecurityTokenHandler();
            //var jwt = handler.ReadJwtToken(accessToken);
            //var nowUtc = DateTime.UtcNow;
            //if (jwt.ValidTo.CompareTo(nowUtc) <= 0)
            //{
            //    //How to use refresh token https://stackoverflow.com/questions/44175115/how-to-use-refresh-token-in-identityserver-4
            //    var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
            //    var response = await _tokenService.GetRefreshToken(refreshToken);
            //    accessToken = response.AccessToken;
            //}


            using (var client = new HttpClient())
            {
                client.SetBearerToken(accessToken);

                var result = client.GetAsync("https://localhost:5445/weatherforecast").Result;

                if (result.IsSuccessStatusCode)
                {
                    var model = result.Content.ReadAsStringAsync().Result;

                    data = JsonConvert.DeserializeObject<List<WeatherData>>(model);

                    return View(data);
                }
                else
                {
                    throw new Exception("Unable to get content");
                }
            }

            // m2m client - client credentials flow
            //using (var client = new HttpClient())
            //{
            //    var tokenResponse = await _tokenService.GetToken("weatherapi.read");

            //    client
            //      .SetBearerToken(tokenResponse.AccessToken);

            //    var result = client
            //      .GetAsync("https://localhost:5445/weatherforecast")
            //      .Result;

            //    if (result.IsSuccessStatusCode)
            //    {
            //        var model = result.Content.ReadAsStringAsync().Result;

            //        data = JsonConvert.DeserializeObject<List<WeatherData>>(model);

            //        return View(data);
            //    }
            //    else
            //    {
            //        throw new Exception("Unable to get content");
            //    }
            //}
            return View(data);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

