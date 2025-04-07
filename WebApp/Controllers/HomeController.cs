using System.Diagnostics;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, HttpClient httpClient, IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public IActionResult Index()
        {

            // Obtener la URL desde appsettings.json
            string apiUrl = _configuration.GetValue<string>("endpoint_api:url");

            // Pasar la URL al frontend a través de ViewData
            ViewData["ApiUrl"] = apiUrl;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public async Task<IActionResult> GetFiles()
        {
            var response = await _httpClient.GetAsync("https://localhost:5001/api/file/get-files");
            if (response.IsSuccessStatusCode)
            {
                var files = await response.Content.ReadAsStringAsync();
                ViewBag.Files = files;
            }
            return View();
        }


    }
}
