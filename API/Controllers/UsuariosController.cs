using System.Data;
using API.Model;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : Controller
    {
        private readonly IConfiguration _configuration;

        public UsuariosController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var usuarios = new List<Dictionary<string, object>>();
            string connectionString = _configuration.GetConnectionString("MySqlConnection");

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_ListarUsuarios", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_UsuarioId", DBNull.Value);
            command.Parameters.AddWithValue("@p_Nombre", DBNull.Value);
            command.Parameters.AddWithValue("@p_Email", DBNull.Value);
            command.Parameters.AddWithValue("@p_Activo", DBNull.Value);
            command.Parameters.AddWithValue("@p_EmpresaId", DBNull.Value);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                }

                usuarios.Add(row);
            }

            return Ok(usuarios); // Esto se serializa automáticamente a JSON
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var usuario = new Dictionary<string, object>();
            string connectionString = _configuration.GetConnectionString("MySqlConnection");

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_ListarUsuarios", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@p_UsuarioId", id);
            command.Parameters.AddWithValue("@p_Nombre", DBNull.Value);
            command.Parameters.AddWithValue("@p_Email", DBNull.Value);
            command.Parameters.AddWithValue("@p_Activo", DBNull.Value);
            command.Parameters.AddWithValue("@p_EmpresaId", DBNull.Value);


            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                }

                usuario = row;
            }

            return Ok(usuario); // Esto se serializa automáticamente a JSON
        }



        [HttpPost]
        public async Task<IActionResult> Post([FromBody] cUsuarioRequest request)
        {
            string mensaje = string.Empty;

            using var connection = new MySqlConnection(_configuration.GetConnectionString("MySqlConnection"));
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_CrearUsuario", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            command.Parameters.AddWithValue("@p_Nombre", request.Nombre);
            command.Parameters.AddWithValue("@p_Email", request.Email);
            command.Parameters.AddWithValue("@p_PasswordHash", passwordHash);
            command.Parameters.AddWithValue("@p_EmpresaId", request.EmpresaId);
            command.Parameters.AddWithValue("@p_RolId", request.RolId);
            command.Parameters.AddWithValue("@p_Activo", request.Activo);

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
        public async Task<IActionResult> ActualizarUsuario([FromBody] mUsuarioRequest request)
        {
            if (request.UsuarioId <= 0)
                return BadRequest(new { mensaje = "El ID del usuario es obligatorio." });

            string mensaje = string.Empty;

            using var connection = new MySqlConnection(_configuration.GetConnectionString("MySqlConnection"));
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_ActualizarUsuario", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            command.Parameters.AddWithValue("@p_UsuarioId", request.UsuarioId);
            command.Parameters.AddWithValue("@p_Nombre", (object?)request.Nombre ?? DBNull.Value);
            command.Parameters.AddWithValue("@p_Email", (object?)request.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("@p_PasswordHash", (object?)passwordHash ?? DBNull.Value);
            command.Parameters.AddWithValue("@p_EmpresaId", (object?)request.EmpresaId ?? DBNull.Value);
            command.Parameters.AddWithValue("@p_RolId", (object?)request.RolId ?? DBNull.Value);
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
        public async Task<IActionResult> Delete([FromBody] eUsuarioRequest request)
        {
            if (request.UsuarioId <= 0)
                return BadRequest(new { mensaje = "UsuarioId inválido." });

            using var connection = new MySqlConnection(_configuration.GetConnectionString("MySqlConnection"));
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_DesactivarUsuario", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@p_UsuarioId", request.UsuarioId);
            command.Parameters.AddWithValue("@p_Activar", request.Activar);

            await command.ExecuteNonQueryAsync();

            string mensaje = request.Activar
                ? "Usuario activado correctamente."
                : "Usuario desactivado correctamente.";

            return Ok(new { mensaje });
        }
    }
}

