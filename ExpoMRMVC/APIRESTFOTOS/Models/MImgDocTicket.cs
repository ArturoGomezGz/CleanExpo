using System;
using System.Collections.Generic;

namespace APIRESTFOTOS.Models
{
    public partial class MImgDocTicket
    {
        public int IdImgDoc { get; set; }
        public int IdTicket { get; set; }
        public byte[] ImgDoc { get; set; } = null!;
        public byte IdTipoArchivo { get; set; }
        public byte Sucursal { get; set; }

        public virtual MTipoArchivo IdTipoArchivoNavigation { get; set; } = null!;
    }
}
