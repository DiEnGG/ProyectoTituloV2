using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

public class MetabaseController : Controller
{
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

        var secret = "fdf23e24b9fbf97c3ee5bd654054f7db09fcc42fee1371c7cec617a84ba26c89";
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

    private static List<string> urlsGuardadas = new();

    [HttpPost]
    public IActionResult GuardarUrl([FromBody] string url)
    {
        if (!string.IsNullOrEmpty(url) && !urlsGuardadas.Contains(url))
        {
            urlsGuardadas.Add(url);
        }
        return Ok();
    }

    [HttpGet]
    public IActionResult ObtenerUrlsGuardadas()
    {
        return Json(urlsGuardadas);
    }

}