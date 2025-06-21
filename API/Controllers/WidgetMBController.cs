using API.Model;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;

namespace API.Controllers
{
    [ApiController]
    [Route("api/widget")]
    public class WidgetMBController : Controller
    {
        private readonly IConfiguration _configuration;

        public WidgetMBController(IConfiguration configuration)
        {

            _configuration = configuration;
        }

        [HttpPost("save-widget")]
        public async Task<IActionResult> saveWidgetMetabase([FromBody] WidgetRequest request)
        {
            // Verificar que el objeto WidgetRequest tenga la estructura esperada
            if (request == null || string.IsNullOrEmpty(request.Url.ToString()) || string.IsNullOrEmpty(request.Nombre.ToString()))
            {
                return BadRequest(new { mensaje = "Datos inválidos" });
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("MySqlConnection");

                

                // Conexión a la base de datos MySQL
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new MySqlCommand("sp_insertWidget", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Parámetros del procedimiento almacenado
                        command.Parameters.AddWithValue("@p_Url", request.Url);
                        command.Parameters.AddWithValue("@p_Nombre", request.Nombre);
                        command.Parameters.AddWithValue("@p_UserId", request.UserId);

                        // Ejecutar el procedimiento almacenado
                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok(new { mensaje = "Widget procesado correctamente." });
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return StatusCode(500, new { mensaje = "Error al crear el Widget", detalle = ex.Message });
            }
        }

        [HttpGet("get-widget")]
        public async Task<IActionResult> GetWidget(int userId)
        {
            List<WidgetRequest> widgetList = new List<WidgetRequest>(); // Lista de objetos CsvMapping para almacenar los resultados
            string connectionString = _configuration.GetConnectionString("MySqlConnection");

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                MySqlCommand command = new MySqlCommand("sp_getWidgets", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@p_userId", userId);

                // Ejecutar el procedimiento almacenado y leer los resultados
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var widget = new WidgetRequest
                        {
                            Url = reader.GetString("url")
                            ,
                            Nombre = reader.GetString("Nombre")
                            ,
                            Empresa = reader.GetString("Empresa")
                        };

                        widgetList.Add(widget); // Agregar el mapeo a la lista
                    }
                }
            }

            // Convertir la lista a JSON
            string jsonResponse = JsonConvert.SerializeObject(widgetList);

            return Ok(jsonResponse); // Retornar los datos en formato JSON
        }

    }
}
