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
            //id_token
            var idtoken = await HttpContext.GetTokenAsync("id_token");
            //access_token
            var token = await HttpContext.GetTokenAsync("access_token");
            //using (var client = new HttpClient())
            //{
            //    client
            //      .SetBearerToken(token);

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

            // m2m client - client credentials flow
            using (var client = new HttpClient())
            {
                var tokenResponse = await _tokenService.GetToken("weatherapi.read");

                client
                  .SetBearerToken(tokenResponse.AccessToken);

                var result = client
                  .GetAsync("https://localhost:5445/weatherforecast")
                  .Result;

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
            return View(data);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

