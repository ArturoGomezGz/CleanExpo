using System;
using System.Collections.Generic;

namespace APIRESTFOTOS.Models
{
    public partial class MFirma
    {
        public int IdFirma { get; set; }
        public byte[] Firma { get; set; } = null!;
        public DateTime? FeModifica { get; set; }
        public string? Usuario { get; set; }
        public string? Obsv { get; set; }
    }
}
