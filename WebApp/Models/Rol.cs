namespace WebApp.Models
{
    public class Rol
    {
        public int RolId { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
    }

    public class eRolRequest
    {
        public int RolId { get; set; }
        public bool Activar { get; set; }
    }
}
