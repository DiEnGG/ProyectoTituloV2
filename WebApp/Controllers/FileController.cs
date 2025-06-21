using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApp.Models;
using System.Drawing;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration.UserSecrets;

[Route("[controller]")]
[Authorize]
public class FileController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IApiService _apiService;
    private readonly IConfiguration _configuration;
    private readonly string _endpoint = "/api/file";

    public FileController(IHttpClientFactory httpClientFactory, IApiService apiService, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _apiService = apiService;
        _configuration = configuration;
    }

    [HttpGet("upload")]
    public async Task<IActionResult> UploadFile() 
    {
        // Obtener la URL de la API desde appsettings.json
        var client = _httpClientFactory.CreateClient();
        var apiUrl = _configuration["endpoint_api:url"];
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var response = await client.GetAsync($"{apiUrl}/api/file/get-categorys?userid={userId}");

        var jsonResponse = await response.Content.ReadAsStringAsync();

        var categorias = JsonConvert.DeserializeObject<List<CategoryResponse>>(jsonResponse);

        ViewBag.Categorias = categorias;


        return View();
    }

    [HttpGet("category")]
    public IActionResult CategoryFile()
    {
        return View();
    }


    [HttpPost("analizar")]
    public async Task<IActionResult> CategoryFile(CategoryFile cf, IFormFile file)
    {
        if (file == null || file.Length == 0 || !ModelState.IsValid)
        {
            //ModelState.AddModelError("File", "Por favor, selecciona un archivo válido.");
            TempData["Mensaje"] = "Por favor, selecciona un archivo válido.";
            TempData["TipoMensaje"] = "error";
            return View();
        }

       // int maxChunkSize = Int32.Parse(_configuration["ChunkSize"]);
        var recommendedMapping = await CSVService.getRecommendedFileMapping(file, _configuration, cf.delimiter);

        // Crear la estructura de datos que se enviará a la API
        cf.UsuarioId = Int32.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var datos = new
        {
            categoria = cf
            ,
            data = recommendedMapping
        };


        // Enviar los datos a la API
        var client = _httpClientFactory.CreateClient();
        var json = JsonConvert.SerializeObject(datos);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");


        // Obtener la URL de la API desde appsettings.json
        var apiUrl = _configuration["endpoint_api:url"]; // Accedemos a la URL configurada

        var response = await client.PostAsync($"{apiUrl}/api/file/create-filecategory", content);

        if (!response.IsSuccessStatusCode)
        {
            //return StatusCode((int)response.StatusCode, "Error al procesar el archivo");
            TempData["Mensaje"] = "Error al procesar el archivo";
            TempData["TipoMensaje"] = "error";
            return View();
        }

        //return Ok(new { mensaje = "Archivo procesado exitosamente" });
        TempData["Mensaje"] = "Archivo procesado exitosamente";
        TempData["TipoMensaje"] = "success"; // puedes usar "error" también

        return View();

    }

    private async Task<string> getFileMappingFromAPI(int CategoryId)
    {
        var client = _httpClientFactory.CreateClient();

        // Obtener la URL base desde el appsettings.json
        var apiUrl = _configuration["endpoint_api:url"];

        // Llamada a la API para obtener el mapeo de columnas
        var response = await client.GetAsync($"{apiUrl}/api/file/get-mapping?CategoryId={CategoryId}");

        if (!response.IsSuccessStatusCode)
        {
            return null; // Si no se puede obtener el mapeo, retornar null
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        //var columnMappings = JsonConvert.DeserializeObject<List<CsvMapping>>(jsonResponse);

        return jsonResponse;
    }

    [HttpPost("EnviarChunk")]
    public async Task<IActionResult> EnviarChunk([FromBody] ChunkRequest request)
    {
        // Agrega este log para depuración en el método EnviarChunk
        //Console.WriteLine($"Recibido chunk para archivo: {request.FileName}, cantidad de filas: {request.Chunk?.Count ?? 0}");
        if (request == null)
            return BadRequest("Request es null");
        if (request.Chunk == null)
            return BadRequest("Chunk es null");
        if (string.IsNullOrEmpty(request.categoryId.ToString()))
            return BadRequest("FileName es null o vacío");
        if (request == null || request.Chunk == null || string.IsNullOrEmpty(request.categoryId.ToString())) 
        {
            return BadRequest("Datos Invalidos");
        }

        var categoryId = request.categoryId;
        var chunk = request.Chunk;
        //List<string> chunk = request.Chunk.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();




        // Obtener los mapeos desde la base de datos
        var columnMappings = await getFileMappingFromAPI(categoryId);
        if (string.IsNullOrEmpty(columnMappings))
        {
            return BadRequest("No se encontraron mapeos para el archivo proporcionado.");
        }

        // Formatear el chunk con los mapeos obtenidos
        var dataFormated = FormatearChunk(chunk, columnMappings);

        // Crear la estructura de datos que se enviará a la API
        CategoryFile cr = new CategoryFile { CategoriaId = categoryId, Nombre = "", Descripcion = "", delimiter = "" };
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var datos = new
        {
            UsuarioId = userId,
            categoria = cr,
            data = dataFormated
        };


        // Enviar los datos a la API
        var client = _httpClientFactory.CreateClient();
        var json = JsonConvert.SerializeObject(datos);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Obtener la URL de la API desde appsettings.json
        var apiUrl = _configuration["endpoint_api:url"]; // Accedemos a la URL configurada

        var response = await client.PostAsync($"{apiUrl}/api/file/procesar", content);
        var errorContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, $"Error al procesar el chunk {errorContent}");
        }

        return Ok(new { mensaje = "Chunk procesado exitosamente" });
    }


    private List<Dictionary<string, object>> FormatearChunk(List<string> chunk, string columnMappingsJson)
    {
        List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();
        
        // Parsear el string JSON de los mapeos de columnas
        var columnMappings = JArray.Parse(columnMappingsJson);

        // Procesar cada fila en el chunk
        foreach (var fila in chunk)
        {
            string delimiter = columnMappings[0]["delimiter"].ToString();
            var valores = fila.Split(delimiter);
            var columnasFormateadas = new Dictionary<string, object>();

            foreach (var mapping in columnMappings)
            {
                var csvColumnName = mapping["CsvColumnName"].ToString();
                var auxColumnName = mapping["AuxColumnName"].ToString();
                var indexCsvColumn = int.Parse(mapping["IndexCsvColumn"].ToString()); // Ajustar al índice 0


                var valor = valores[indexCsvColumn].Trim();

                var isHeader = valor.Equals(csvColumnName);

                if (!isHeader)
                {
                    if (auxColumnName.StartsWith("AuxString"))
                    {
                        columnasFormateadas[auxColumnName] = valor;
                    }
                    else if (auxColumnName.StartsWith("AuxDateTime"))
                    {
                        DateTime dateValor = DateTime.Parse(valor);
                        columnasFormateadas[auxColumnName] = dateValor.ToString("yyyy-MM-dd");
                    }
                    else if (auxColumnName.StartsWith("AuxDecimal"))
                    {
                        //decimal decimalValor = decimal.Parse(valor);
                        string decimalValor = valor;
                        columnasFormateadas[auxColumnName] = decimalValor;
                    }
                }
            }
            if (columnasFormateadas.Count > 0)
            {
                dataList.Add(columnasFormateadas);
            }
        }

        return dataList;
    }

    [HttpGet("VisorArchivos")]
    public async Task<IActionResult> VisorArchivos()
    {
        var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var archivos = await _apiService.GetAsync<Archivo>($"{_endpoint}/get-archivos?UserId={usuarioId}");
        return View(archivos); // Retorna una lista de Archivos
    }

}
