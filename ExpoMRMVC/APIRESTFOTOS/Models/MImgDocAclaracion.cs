using System;
using System.Collections.Generic;

namespace APIRESTFOTOS.Models
{
    public partial class MImgDocAclaracion
    {
        public int IdImgDoc { get; set; }
        public int IdAclaracion { get; set; }
        public byte[] ImgDoc { get; set; } = null!;
        public byte IdTipoArchivo { get; set; }
        public byte Sucursal { get; set; }
        public bool EvidenciaMr { get; set; }
    }
}
