using System;
using System.Collections.Generic;

namespace APIRESTFOTOS.Models
{
    public partial class MImgDoc
    {
        public int IdImgDoc { get; set; }
        public byte[] ImgDoc { get; set; } = null!;
        public byte IdTipoArchivo { get; set; }

        public virtual MTipoArchivo IdTipoArchivoNavigation { get; set; } = null!;
    }
}
