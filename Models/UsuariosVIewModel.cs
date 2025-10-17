using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ExpoMR.Models
{
    public class UsuarioExpo
    {
        public int IdEmail { get; set; }

        public string Nombre { get; set; }

        public string Email { get; set; }

        public int IdTipoPersona { get; set; }

        public string TipoPersona { get; set; }


        public bool Usuario { get; set; }

        public bool Lock { get; set; }

        public string Sucursal { get; set; }

        public string Ruta { get; set; }

        public string Invitacion { get; set; }

        public bool InvMasiva { get; set; }
    }

    public class UsuariosNotificacion
    {
        public int IdPersona { get; set; }

        public string Nombre { get; set; }

        public byte Notificado { get; set; }

        public int IdEmail { get; set; }

        public string Email { get; set; }
    }
}