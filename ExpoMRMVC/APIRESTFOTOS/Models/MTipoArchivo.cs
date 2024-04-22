using System;
using System.Collections.Generic;

namespace APIRESTFOTOS.Models
{
    public partial class MTipoArchivo
    {
        public MTipoArchivo()
        {
            MImgDocTickets = new HashSet<MImgDocTicket>();
            MImgDocs = new HashSet<MImgDoc>();
        }

        public byte IdTipoArchivo { get; set; }
        public string TipoArchivo { get; set; } = null!;

        public virtual ICollection<MImgDocTicket> MImgDocTickets { get; set; }
        public virtual ICollection<MImgDoc> MImgDocs { get; set; }
    }
}
