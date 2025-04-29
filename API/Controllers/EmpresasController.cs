using System.Data;
using API.Model;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpresasController : Controller
    {
        private readonly IConfiguration _configuration;

        public EmpresasController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var empresas = new List<Dictionary<string, object>>();
            string connectionString = _configuration.GetConnectionString("MySqlConnection");

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_ListarEmpresas", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_EmpresaId", DBNull.Value);
            

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                }

                empresas.Add(row);
            }

            return Ok(empresas); // Esto se serializa automáticamente a JSON
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var empresa = new Dictionary<string, object>();
            string connectionString = _configuration.GetConnectionString("MySqlConnection");

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_ListarEmpresas", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_EmpresaId", id);
            


            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                }

                empresa = row;
            }

            return Ok(empresa); // Esto se serializa automáticamente a JSON
        }




        [HttpPost]
        public async Task<IActionResult> Post([FromBody] cEmpresaRequest request)
        {
            string mensaje = string.Empty;

            using var connection = new MySqlConnection(_configuration.GetConnectionString("MySqlConnection"));
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_CrearEmpresa", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@p_Nombre", request.Nombre);
            command.Parameters.AddWithValue("@p_RazonSocial", request.RazonSocial);


            var outputParam = new MySqlParameter("@p_Mensaje", MySqlDbType.VarChar, 255)
            {
                Direction = ParameterDirection.Output
            };

            command.Parameters.Add(outputParam);

            await command.ExecuteNonQueryAsync();

            mensaje = outputParam.Value?.ToString();

            return Ok(new { mensaje });
        }



        [HttpPut]
        public async Task<IActionResult> ActualizarEmpresa([FromBody] mEmpresaRequest request)
        {
            if (request.EmpresaId <= 0)
                return BadRequest(new { mensaje = "El ID de la empresa es obligatorio." });

            string mensaje = string.Empty;

            using var connection = new MySqlConnection(_configuration.GetConnectionString("MySqlConnection"));
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_ActualizarEmpresa", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@p_EmpresaId", request.EmpresaId);
            command.Parameters.AddWithValue("@p_Nombre", (object?)request.Nombre ?? DBNull.Value);
            command.Parameters.AddWithValue("@p_RazonSocial", (object?)request.RazonSocial ?? DBNull.Value);
            command.Parameters.AddWithValue("@p_Activo", (object?)request.Activo ?? DBNull.Value);

            var outputParam = new MySqlParameter("@p_Mensaje", MySqlDbType.VarChar, 255)
            {
                Direction = ParameterDirection.Output
            };

            command.Parameters.Add(outputParam);

            await command.ExecuteNonQueryAsync();

            mensaje = outputParam.Value?.ToString();

            return Ok(new { mensaje });
        }




        [HttpPost("del")]
        public async Task<IActionResult> Delete([FromBody] eEmpresaRequest request)
        {
            if (request.EmpresaId <= 0)
                return BadRequest(new { mensaje = "EmpresaId inválido." });

            using var connection = new MySqlConnection(_configuration.GetConnectionString("MySqlConnection"));
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_DesactivarEmpresa", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@p_EmpresaId", request.EmpresaId);
            command.Parameters.AddWithValue("@p_Activar", request.Activar);

            await command.ExecuteNonQueryAsync();

            string mensaje = request.Activar
                ? "Empresa activado correctamente."
                : "Empresa desactivado correctamente.";

            return Ok(new { mensaje });
        }


    }
}
