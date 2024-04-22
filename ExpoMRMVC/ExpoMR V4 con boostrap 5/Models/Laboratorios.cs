using System.ComponentModel.DataAnnotations;

namespace ExpoMR.Models
{
    public class mLaboratorio
    {
        [Key]
        public int IdLaboratorio { get; set; }
        public string Laboratorio { get; set; }
    }

    public class mLaboratorioI
    {
        [Key]
        public Int16 IdLaboratorio { get; set; }
        public string Laboratorio { get; set; }
    }
}
