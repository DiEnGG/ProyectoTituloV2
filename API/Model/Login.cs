namespace API.Model
{
    
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string EmpresaNombre { get; set; }
        public string RolNombre { get; set; }
        public bool Activo { get; set; }
        public string Mensaje { get; set; }
        public bool Exito { get; set; }
    }


}
