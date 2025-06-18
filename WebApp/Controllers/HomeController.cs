using System.Diagnostics;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WebApp.Models;

namespace WebApp.Controllers
{
    [Authorize]
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
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");

            string apiUrl = _configuration.GetValue<string>("endpoint_api:url");
            ViewData["ApiUrl"] = apiUrl;

            // Nombre
            ViewBag.NombreUsuario = User.Identity.Name;

            // Rol (si tienes roles en Claims)
            var rolClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role);
            ViewBag.RolUsuario = rolClaim?.Value ?? "Usuario";

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
