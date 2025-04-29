using System.Data;
using API.Model;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using static API.Model.RolRequest;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : Controller
    {
        private readonly IConfiguration _configuration;

        public RolesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var roles = new List<Dictionary<string, object>>();
            string connectionString = _configuration.GetConnectionString("MySqlConnection");

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_ListarRoles", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_RolId", null);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                }

                roles.Add(row);
            }

            return Ok(roles); // Esto se serializa automáticamente a JSON
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var rol = new Dictionary<string, object>();
            string connectionString = _configuration.GetConnectionString("MySqlConnection");

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_ListarRoles", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_RolId", id);


            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                }

                rol = row;
            }

            return Ok(rol); // Esto se serializa automáticamente a JSON
        }



        [HttpPost]
        public async Task<IActionResult> Post([FromBody] cRolRequest request)
        {
            string mensaje = string.Empty;

            using var connection = new MySqlConnection(_configuration.GetConnectionString("MySqlConnection"));
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_CrearRol", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@p_Nombre", request.Nombre);
            

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
        public async Task<IActionResult> ActualizarRol([FromBody] mRolRequest request)
        {
            if (request.RolId <= 0)
                return BadRequest(new { mensaje = "El ID del rol es obligatorio." });

            string mensaje = string.Empty;

            using var connection = new MySqlConnection(_configuration.GetConnectionString("MySqlConnection"));
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_ActualizarRol", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
     

            command.Parameters.AddWithValue("@p_RolId", request.RolId);
            command.Parameters.AddWithValue("@p_Nombre", (object?)request.Nombre ?? DBNull.Value);
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
        public async Task<IActionResult> Delete([FromBody] eRolRequest request)
        {
            if (request.RolId <= 0)
                return BadRequest(new { mensaje = "RolId inválido." });

            using var connection = new MySqlConnection(_configuration.GetConnectionString("MySqlConnection"));
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_DesactivarRol", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@p_RolId", request.RolId);
            command.Parameters.AddWithValue("@p_Activar", request.Activar);

            await command.ExecuteNonQueryAsync();

            string mensaje = request.Activar
                ? "Rol activado correctamente."
                : "Rol desactivado correctamente.";

            return Ok(new { mensaje });
        }


    }

}
