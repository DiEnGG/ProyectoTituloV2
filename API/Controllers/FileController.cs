using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Printing;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using API.Model;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FileController(IConfiguration configuration) {

            _configuration = configuration;
        }

        [HttpGet("get-files")]
        public IActionResult GetFiles()
        {
            List<object> files = new List<object>();
            string connectionString = _configuration.GetConnectionString("MySqlConnection");

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM csvdata"; // Ajusta la consulta según tu tabla
                using (var cmd = new MySqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        files.Add(new
                        {
                            Id = reader.GetInt32("Id"),
                            FileName = reader.GetString("FileName"),
                            AuxString1 = reader.IsDBNull(reader.GetOrdinal("AuxString1")) ? null : reader.GetString("AuxString1"),
                            AuxString2 = reader.IsDBNull(reader.GetOrdinal("AuxString2")) ? null : reader.GetString("AuxString2"),
                            AuxString3 = reader.IsDBNull(reader.GetOrdinal("AuxString3")) ? null : reader.GetString("AuxString3")
                        });
                    }
                }
            }
            return Ok(files);
        }

        [HttpGet("get-mapping")]
        public async Task<IActionResult> GetFileMapping(string fileName)
        {
            List<CsvMapping> files = new List<CsvMapping>(); // Lista de objetos CsvMapping para almacenar los resultados
            string connectionString = _configuration.GetConnectionString("MySqlConnection");

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                MySqlCommand command = new MySqlCommand("sp_getFileMapping", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@filename", fileName);

                // Ejecutar el procedimiento almacenado y leer los resultados
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var mapping = new CsvMapping
                        {
                            //Id = reader.GetInt32("Id")
                            CategoriaId = reader.GetInt32("CategoriaId")
                            ,CsvColumnName = reader.GetString("CsvColumnName")
                            ,AuxColumnName = reader.GetString("AuxColumnName")
                            ,IndexCsvColumn = reader.GetInt32("IndexCsvColumn")
                            ,CreatedAt = reader.GetDateTime("CreatedAt")
                            
                        };

                        files.Add(mapping); // Agregar el mapeo a la lista
                    }
                }
            }

            // Convertir la lista a JSON
            string jsonResponse = JsonConvert.SerializeObject(files);

            return Ok(jsonResponse); // Retornar los datos en formato JSON
        }


        [HttpPost("procesar")]
        public async Task<IActionResult> ProcesarChunk([FromBody] CsvRequest request)
        {
            // Verificar que el objeto CsvRequest tenga la estructura esperada
            if (request == null || string.IsNullOrEmpty(request.FileName) || request.data == null)
            {
                return BadRequest(new { mensaje = "Datos inválidos" });
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("MySqlConnection");

                // Crear el objeto JSON a partir de las columnas recibidas
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(request.data);

                // Conexión a la base de datos MySQL
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new MySqlCommand("sp_insertFileData", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Parámetros del procedimiento almacenado
                        command.Parameters.AddWithValue("@filename", request.FileName);
                        command.Parameters.AddWithValue("@data", json);

                        // Ejecutar el procedimiento almacenado
                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok(new { mensaje = "Chunk procesado" });
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return StatusCode(500, new { mensaje = "Error al procesar el chunk", detalle = ex.Message });
            }
        }





        [HttpPost("create-filecategory")]
        public async Task<IActionResult> CreateFileCategory([FromBody] CsvRequest request)
        {
            // Verificar que el objeto CsvRequest tenga la estructura esperada
            if (request == null || string.IsNullOrEmpty(request.FileName) || request.data == null)
            {
                return BadRequest(new { mensaje = "Datos inválidos" });
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("MySqlConnection");

                // Crear el objeto JSON a partir de las columnas recibidas
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(request.data);

                // Conexión a la base de datos MySQL
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new MySqlCommand("sp_insertFileCategory", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Parámetros del procedimiento almacenado
                        command.Parameters.AddWithValue("@filename", request.FileName);
                        command.Parameters.AddWithValue("@data", json);

                        // Ejecutar el procedimiento almacenado
                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok(new { mensaje = "Archivo procesado" });
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return StatusCode(500, new { mensaje = "Error al procesar el archivo", detalle = ex.Message });
            }
        }





    }
}
