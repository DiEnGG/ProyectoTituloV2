namespace API.Model
{
    public class RolRequest
    {
        public class cRolRequest
        {
            public string Nombre { get; set; }
        }

        public class mRolRequest
        {
            public int? RolId { get; set; }
            public string? Nombre { get; set; }

            public bool? Activo { get; set; }
        }

        public class eRolRequest
        {
            public int RolId { get; set; }
            public bool Activar { get; set; }
        }
    }
}
