namespace WebApp.Models
{
    public class Empresa
    {
        public int EmpresaId { get; set; }
        public string Nombre { get; set; }
        public string RazonSocial { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public class eEmpresaRequest
    {
        public int EmpresaId { get; set; }
        public bool Activar { get; set; }
    }
}
