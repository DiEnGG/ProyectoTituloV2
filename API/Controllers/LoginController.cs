using System.Data;
using API.Model;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace API.Controllers
{
    [ApiController]
    [Route("auth/[controller]")]
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {

            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            using var connection = new MySqlConnection(_configuration.GetConnectionString("MySqlConnection"));
            await connection.OpenAsync();

            using var command = new MySqlCommand("sp_LoginUsuario", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@p_Email", request.Email);

            var outputUsuarioId = new MySqlParameter("@p_UsuarioId", MySqlDbType.Int32) { Direction = ParameterDirection.Output };
            var outputNombre = new MySqlParameter("@p_Nombre", MySqlDbType.VarChar, 255) { Direction = ParameterDirection.Output };
            var outputPasswordHash = new MySqlParameter("@p_PasswordHash", MySqlDbType.VarChar, 255) { Direction = ParameterDirection.Output };
            var outputEmpresaId = new MySqlParameter("@p_EmpresaId", MySqlDbType.Int32) { Direction = ParameterDirection.Output };
            var outputRolId = new MySqlParameter("@p_RolId", MySqlDbType.Int32) { Direction = ParameterDirection.Output };
            var outputActivo = new MySqlParameter("@p_Activo", MySqlDbType.Bit) { Direction = ParameterDirection.Output };

            command.Parameters.Add(outputUsuarioId);
            command.Parameters.Add(outputNombre);
            command.Parameters.Add(outputPasswordHash);
            command.Parameters.Add(outputEmpresaId);
            command.Parameters.Add(outputRolId);
            command.Parameters.Add(outputActivo);

            await command.ExecuteNonQueryAsync();

            var passwordHash = outputPasswordHash.Value?.ToString();

            if (string.IsNullOrEmpty(passwordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, passwordHash))
            {
                return Unauthorized(new LoginResponse { Exito = false, Mensaje = "Credenciales inválidas." });
            }

            return Ok(new LoginResponse
            {
                Exito = true,
                Mensaje = "Inicio de sesión exitoso.",
                UsuarioId = Convert.ToInt32(outputUsuarioId.Value),
                Nombre = outputNombre.Value.ToString(),
                EmpresaId = Convert.ToInt32(outputEmpresaId.Value),
                RolId = Convert.ToInt32(outputRolId.Value),
                Activo = Convert.ToBoolean(outputActivo.Value)
            });
        }

    }
}
