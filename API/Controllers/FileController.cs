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

        public FileController(IConfiguration configuration)
        {

            _configuration = configuration;
        }

        [HttpGet("get-mapping")]
        public async Task<IActionResult> GetFileMapping(int CategoryId)
        {
            List<CsvMapping> files = new List<CsvMapping>(); // Lista de objetos CsvMapping para almacenar los resultados
            string connectionString = _configuration.GetConnectionString("MySqlConnection");

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                MySqlCommand command = new MySqlCommand("sp_getFileMapping", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@p_CategoryId", CategoryId);

                // Ejecutar el procedimiento almacenado y leer los resultados
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var mapping = new CsvMapping
                        {
                            //Id = reader.GetInt32("Id")
                            CategoriaId = reader.GetInt32("CategoriaId")
                            ,
                            CsvColumnName = reader.GetString("CsvColumnName")
                            ,
                            AuxColumnName = reader.GetString("AuxColumnName")
                            ,
                            IndexCsvColumn = reader.GetInt32("IndexCsvColumn")
                            ,
                            CreatedAt = reader.GetDateTime("CreatedAt")
                            ,
                            delimiter = reader.GetString("delimiter")

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
            if (request == null || string.IsNullOrEmpty(request.categoria.CategoriaId.ToString()) || request.data == null)
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
                        command.Parameters.AddWithValue("@p_userId", request.UsuarioId);
                        command.Parameters.AddWithValue("@p_categoryId", request.categoria.CategoriaId);
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
            if (request == null || string.IsNullOrEmpty(request.categoria.Nombre) || request.data == null)
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
                        command.Parameters.AddWithValue("@userId", request.categoria.UsuarioId);
                        command.Parameters.AddWithValue("@filename", request.categoria.Nombre);
                        command.Parameters.AddWithValue("@fileDesc", request.categoria.Descripcion);
                        command.Parameters.AddWithValue("@fileDelimiter", request.categoria.delimiter);
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

        [HttpGet("get-categorys")]
        public async Task<IActionResult> GetCategorys(int userid)
        {
            ///var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<CategoryResponse> categoryList = new List<CategoryResponse>();
            string connectionString = _configuration.GetConnectionString("MySqlConnection");

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                MySqlCommand command = new MySqlCommand("sp_ListarCategoriasArchivos", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@p_UserId", userid);

                // Ejecutar el procedimiento almacenado y leer los resultados
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var category = new CategoryResponse
                        {
                            CategoriaId = reader.GetInt32("CategoriaId"),
                            Nombre = reader.GetString("Nombre"),
                            Descripcion = reader.GetString("Descripcion"),
                            EmpresaId = reader.GetInt32("EmpresaId"),
                            NombreEmpresa = reader.GetString("NombreEmpresa")
                        };

                        categoryList.Add(category); // Agregar el mapeo a la lista
                    }
                }
            }

            // Convertir la lista a JSON
            string jsonResponse = JsonConvert.SerializeObject(categoryList);

            return Ok(jsonResponse); // Retornar los datos en formato JSON
        }

        [HttpGet("get-archivos")]
        public async Task<IActionResult> GetArchivos(int UserId)
        {
            ///var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<ArchivoResponse> archivoList = new List<ArchivoResponse>();
            string connectionString = _configuration.GetConnectionString("MySqlConnection");

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                MySqlCommand command = new MySqlCommand("sp_ListarArchivosPorEmpresa", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@p_UserId", UserId);

                // Ejecutar el procedimiento almacenado y leer los resultados
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var archivo = new ArchivoResponse
                        {
                            ArchivoId = reader.GetInt32("ArchivoId"),
                            NombreOriginal = reader.GetString("NombreOriginal"),
                            RutaAlmacenamiento = reader.IsDBNull("RutaAlmacenamiento") ? "" : reader.GetString("RutaAlmacenamiento"),
                            FechaSubida = reader.GetDateTime("FechaSubida"),
                            NombreEmpresa = reader.GetString("NombreEmpresa"),
                            NombreUsuario = reader.GetString("NombreUsuario"),
                            NombreCategoria = reader.GetString("NombreCategoria"),
                        };

                        archivoList.Add(archivo); // Agregar el mapeo a la lista
                    }
                }
            }

            // Convertir la lista a JSON
            string jsonResponse = JsonConvert.SerializeObject(archivoList);

            return Ok(jsonResponse); // Retornar los datos en formato JSON
        }
    }
}
