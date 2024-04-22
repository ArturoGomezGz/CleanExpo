using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace APIRESTFOTOS.Models
{
    public class MFoto
    {
        [Column(TypeName = "smallint")]
        public int IdFoto { get; set; }
        [Column(TypeName = "smallint")]
        public int IdProducto { get; set; }
        public byte[] Foto { get; set; } = null!;
        [Column(TypeName = "tinyint")]
        public int Atributos { get; set; }
        public string? Obvs { get; set; }
        public DateTime FeModifica { get; set; }
        //public string UrlImagen { get; set; }
    }
}
