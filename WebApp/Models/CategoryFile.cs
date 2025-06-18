namespace WebApp.Models
{
    public class CategoryFile
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        //public int EmpresaId { get; set; }
        public int UsuarioId { get; set; }
        //public DateTime FechaCreacion { get; set; }
        public string delimiter { get; set; }

    }

    public class CategoryResponse
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int EmpresaId { get; set; }
        public string NombreEmpresa { get; set; }
    }
}
