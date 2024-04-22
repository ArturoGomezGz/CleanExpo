using System;
using System.Collections.Generic;

namespace APIRESTFOTOS.Models
{
    public partial class MImgGruposCofepri
    {
        public byte IdImagen { get; set; }
        public byte[] Imagen { get; set; } = null!;
        public bool TipoAgrupacion { get; set; }
        public byte IdAgrupacion { get; set; }
    }
}
