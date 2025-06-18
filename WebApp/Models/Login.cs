using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }


    public class LoginResponse
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public int EmpresaId { get; set; }
        public int RolId { get; set; }
        public bool Activo { get; set; }
        public string Mensaje { get; set; }
        public bool Exito { get; set; }
    }




}
