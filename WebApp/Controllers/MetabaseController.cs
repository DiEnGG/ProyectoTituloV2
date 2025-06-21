using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using WebApp.Models;
using System.Security.Claims;
using System.Net.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;

[Authorize]
public class MetabaseController : Controller
{

    private readonly IConfiguration _configuration;
    private readonly string _endpoint = "/api/widget";
    private readonly IHttpClientFactory _httpClientFactory;

    public MetabaseController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }


    public IActionResult Editor()
    {
        var token = GenerateMetabaseToken();
        var iframeUrl = $"http://localhost:3000/embed/question/{token}#bordered=true&titled=true";
        ViewBag.IframeUrl = iframeUrl;
        return View();
    }
    [HttpGet]
    public IActionResult VistaIframe(string tipo = "dashboard", int id = 1)
    {
        var token = GenerateMetabaseToken(tipo, id);
        var iframeUrl = $"http://localhost:3000/embed/{tipo}/{token}#bordered=true&titled=true";
        ViewBag.IframeUrl = iframeUrl;
        ViewBag.Tipo = tipo;
        ViewBag.Id = id;
        return View("VistaIframe");
    }


    private string GenerateMetabaseToken(string tipo = "dashboard", int id = 1)
    {
        var resource = new Dictionary<string, object>();
        if (tipo == "dashboard")
            resource["dashboard"] = id;
        else if (tipo == "question")
            resource["question"] = id;
        else
            throw new ArgumentException("Tipo inválido: debe ser 'dashboard' o 'question'");

        var payload = new Dictionary<string, object>
        {
            ["resource"] = resource,
            ["params"] = new Dictionary<string, object>(),
            ["exp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (365 * 24 * 60 * 60)//1 año de expiración
        };

        var secret = "29508034cebff3ce5df8f769981fb6707432f901943e5fabb0551d109e711005";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var header = new JwtHeader(credentials);
        var payloadJson = JsonConvert.SerializeObject(payload);
        var jwtPayload = JwtPayload.Deserialize(payloadJson);

        var secToken = new JwtSecurityToken(header, jwtPayload);
        var handler = new JwtSecurityTokenHandler();

        return handler.WriteToken(secToken);
    }

    public class GuardarPlantillaDto
    {
        public string Nombre { get; set; }
        public string Contenido { get; set; }
        public string Css { get; set; }
        public string ImagenBase64 { get; set; } // <-- NUEVO
    }

    [HttpPost]
    public IActionResult GuardarPlantilla([FromBody] GuardarPlantillaDto data)
    {
        if (data == null || string.IsNullOrWhiteSpace(data.Nombre) || string.IsNullOrWhiteSpace(data.Contenido))
            return BadRequest(new { mensaje = "Faltan datos requeridos." });

        var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Plantillas");
        if (!Directory.Exists(carpeta))
            Directory.CreateDirectory(carpeta);

        var safeName = string.Join("_", data.Nombre.Split(Path.GetInvalidFileNameChars()));
        if (string.IsNullOrWhiteSpace(safeName))
            return BadRequest(new { mensaje = "El nombre de plantilla es inválido." });

        try
        {
            // Guarda HTML
            System.IO.File.WriteAllText(Path.Combine(carpeta, safeName + ".html"), data.Contenido);

            // Guarda CSS
            System.IO.File.WriteAllText(Path.Combine(carpeta, safeName + ".css"), data.Css ?? "");

            // Guarda la imagen (thumbnail)
            if (!string.IsNullOrEmpty(data.ImagenBase64))
            {
                // Quita el encabezado de dataURL si existe
                var base64Data = data.ImagenBase64.Contains(",") ? data.ImagenBase64.Split(',')[1] : data.ImagenBase64;
                var bytes = Convert.FromBase64String(base64Data);
                System.IO.File.WriteAllBytes(Path.Combine(carpeta, safeName + ".png"), bytes);
            }

            return Ok(new { mensaje = "Plantilla guardada correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al guardar la plantilla.", detalle = ex.Message });
        }
    }

    //[HttpPost]
    //public IActionResult GuardarPlantilla([FromBody] GuardarPlantillaDto data)
    //{
    //    var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Plantillas");
    //    if (!Directory.Exists(carpeta))
    //        Directory.CreateDirectory(carpeta);

    //    var safeName = string.Join("_", data.Nombre.Split(Path.GetInvalidFileNameChars()));

    //    // Guarda HTML
    //    System.IO.File.WriteAllText(Path.Combine(carpeta, safeName + ".html"), data.Contenido);

    //    // Guarda CSS
    //    System.IO.File.WriteAllText(Path.Combine(carpeta, safeName + ".css"), data.Css);

    //    return Ok();
    //}

    //public class GuardarPlantillaDto
    //{
    //    public string Contenido { get; set; }
    //    public string Css { get; set; }
    //    public string Nombre { get; set; }
    //}
    public IActionResult ObtenerPlantillas()
    {
        var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Plantillas");
        var archivos = Directory.GetFiles(carpeta, "*.html")
            .Select(x => Path.GetFileNameWithoutExtension(x))
            .ToList();
        return Json(archivos);
    }

    public IActionResult ObtenerPlantilla(string nombre)
    {
        var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Plantillas");
        var safeName = string.Join("_", nombre.Split(Path.GetInvalidFileNameChars()));
        var html = System.IO.File.ReadAllText(Path.Combine(carpeta, safeName + ".html"));
        var css = System.IO.File.ReadAllText(Path.Combine(carpeta, safeName + ".css"));
        return Json(new { html, css });
    }



    public async Task<IActionResult> DescargarPdf(string nombre)
    {
        var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Plantillas");
        var safeName = string.Join("_", nombre.Split(Path.GetInvalidFileNameChars()));
        var htmlPath = Path.Combine(carpeta, safeName + ".html");
        var cssPath = Path.Combine(carpeta, safeName + ".css");

        if (!System.IO.File.Exists(htmlPath) || !System.IO.File.Exists(cssPath))
        {
            Console.WriteLine($"❌ Plantilla '{nombre}' no encontrada en '{carpeta}'");
            return NotFound("Plantilla no encontrada");
        }

        var html = System.IO.File.ReadAllText(htmlPath);
        var css = System.IO.File.ReadAllText(cssPath);

        // Log de lo que se enviará
        Console.WriteLine("----- Enviando a Puppeteer PDF Service -----");
        Console.WriteLine($"Nombre plantilla: {nombre}");
        Console.WriteLine($"HTML (primeros 500 chars): {html.Substring(0, Math.Min(html.Length, 5000))}");
        Console.WriteLine($"CSS (primeros 500 chars): {css.Substring(0, Math.Min(css.Length, 5000))}");
        Console.WriteLine("--------------------------------------------");

        using var client = new HttpClient();
        var payload = new { html, css };
        var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        Console.WriteLine($"POST http://localhost:3005/pdf con payload de {jsonPayload.Length} caracteres");
        var response = await client.PostAsync("http://localhost:3005/pdf", content);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"❌ Error al generar PDF: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            return StatusCode(500, "Error generando el PDF");
        }

        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        Console.WriteLine($"✅ PDF recibido, tamaño: {pdfBytes.Length} bytes");

        return File(pdfBytes, "application/pdf", $"{nombre}.pdf");
    }
    [HttpGet]
    public IActionResult ExistePlantilla(string nombre)
    {
        var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Plantillas");
        var safeName = string.Join("_", nombre.Split(Path.GetInvalidFileNameChars()));

        var htmlPath = Path.Combine(carpeta, safeName + ".html");
        var cssPath = Path.Combine(carpeta, safeName + ".css");

        bool existe = System.IO.File.Exists(htmlPath) && System.IO.File.Exists(cssPath);
        return Json(existe);
    }

    //private static List<UrlGuardada> urlsGuardadas = new List<UrlGuardada>();

    [HttpPost]
    public async Task<IActionResult> GuardarUrl([FromBody] WidgetRequest data)
    {
        // Chequea que venga bien y no esté repetido
        if (data != null && !string.IsNullOrEmpty(data.Url) && !string.IsNullOrEmpty(data.Nombre))
        {
            var client = _httpClientFactory.CreateClient();
            // Solo agrega si NO existe ya una con esa URL y nombre
            var userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Obtener la URL de la API desde appsettings.json
            var apiUrl = _configuration["endpoint_api:url"]; // Accedemos a la URL configurada

            data.UserId = userId;

            var json = JsonConvert.SerializeObject(data);

            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{apiUrl}{_endpoint}/save-widget", content);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(500, "Error creando el Widget");
            }
            return Ok();
        }
        return StatusCode(500, "Error creando el Widget");
    }


    [HttpGet]
    public async Task<string> ObtenerUrlsGuardadas()
    {

        var client = _httpClientFactory.CreateClient();

        // Obtener la URL base desde el appsettings.json
        var apiUrl = _configuration["endpoint_api:url"];


        var userId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // Llamada a la API para obtener el mapeo de columnas
        var response = await client.GetAsync($"{apiUrl}{_endpoint}/get-widget?userId={userId}");

        if (!response.IsSuccessStatusCode)
        {
            return null; // Si no se puede obtener el mapeo, retornar null
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        //var columnMappings = JsonConvert.DeserializeObject<List<CsvMapping>>(jsonResponse);

        return jsonResponse;
    }


}