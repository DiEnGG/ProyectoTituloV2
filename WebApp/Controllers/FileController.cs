using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApp.Models;
using System.Drawing;
using Microsoft.AspNetCore.Authorization.Infrastructure;

[Route("[controller]")]
public class FileController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public FileController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }


    public IActionResult Index() {
        return View();
    }

    private async Task<string> getFileMapping(string fileName)
    {
        var client = _httpClientFactory.CreateClient();

        // Obtener la URL base desde el appsettings.json
        var apiUrl = _configuration["endpoint_api:url"];

        // Llamada a la API para obtener el mapeo de columnas
        var response = await client.GetAsync($"{apiUrl}/api/file/get-mapping?fileName={fileName}");

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
        if (request == null || request.Chunk == null || string.IsNullOrEmpty(request.FileName)) 
        {
            return BadRequest("Datos Invalidos");
        }

        var fileName = request.FileName;
        var chunk = request.Chunk;

        // Obtener los mapeos desde la base de datos
        var columnMappings = await getFileMapping(fileName);
        if (string.IsNullOrEmpty(columnMappings))
        {
            return BadRequest("No se encontraron mapeos para el archivo proporcionado.");
        }

        // Formatear el chunk con los mapeos obtenidos
        var dataFormated = FormatearChunk(chunk, columnMappings);

        // Crear la estructura de datos que se enviará a la API
        var datos = new
        {
            fileName
            ,columnas = dataFormated
        };


        // Enviar los datos a la API
        var client = _httpClientFactory.CreateClient();
        var json = JsonConvert.SerializeObject(datos);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Obtener la URL de la API desde appsettings.json
        var apiUrl = _configuration["endpoint_api:url"]; // Accedemos a la URL configurada

        var response = await client.PostAsync($"{apiUrl}/api/file/procesar", content);

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, "Error al procesar el chunk");
        }

        return Ok(new { mensaje = "Chunk procesado exitosamente" });
    }


    private Dictionary<string, object> FormatearChunk(List<string> chunk, string columnMappingsJson)
    {
        var columnasFormateadas = new Dictionary<string, object>();

        // Parsear el string JSON de los mapeos de columnas
        var columnMappings = JArray.Parse(columnMappingsJson);

        // Procesar cada fila en el chunk
        foreach (var fila in chunk)
        {

            var valores = fila.Split(',');

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
                        decimal decimalValor = decimal.Parse(valor);
                        columnasFormateadas[auxColumnName] = decimalValor;
                    }
                }
            }
        }

        return columnasFormateadas;
    }
}
