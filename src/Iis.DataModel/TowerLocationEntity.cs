using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Iis.DataModel
{
    public class TowerLocationEntity
    {
        public int Id { get; set; }

        public string DataSource { get; set; }

        public string RadioType { get; set; }
        public string Mcc { get; set; }

        public string Mnc { get; set; }

        public string Lac { get; set; }

        public string CellId { get; set; }

        public decimal Lat { get; set; }

        public decimal Long { get; set; }

        public string Range { get; set; }

        public DateTime Created { get; set; }
        
        public DateTime? Updated { get; set; }
    }
}