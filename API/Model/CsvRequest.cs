namespace API.Model
{
    public class CategoryRequest
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string delimiter { get; set; }
        public int UsuarioId { get; set; }
    }
    public class CsvRequest
    {
        public CategoryRequest categoria { get; set; }
        public List<Dictionary<string, string>> data { get; set; }

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
