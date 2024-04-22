using Microsoft.AspNetCore.Identity;

namespace ExpoMR.Models
{   
    public class Img : IdentityUser
    {
        public int idUsuario { get; set; }
        public Int32 Proveedor { get; set; }
        public string Nombre { get; set; }
        public byte[] img { get; set; }
        public byte[] fondo { get; set; }
        public string Description { get; set; }
        public int color { get; set; }
    }

    public class Users
    { 
        public string Nombre { get; set; }
        public string Email { get; set; }
        public int TipoUsuario { get; set; }
        public int Vip { get; set; }
        public int Usuario { get; set; }
        public string Sucursal { get; set; }
        public string Ruta { get; set; }
        public string invitacion { get; set; }

    }
}
