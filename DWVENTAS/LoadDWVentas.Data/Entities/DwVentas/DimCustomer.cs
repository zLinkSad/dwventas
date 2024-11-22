

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoadDWVentas.Data.Entities.DwVentas
{
    [Table("DimCustomers")]
    public class DimCustomers
    {
        [Key]
        public int CustomerID { get; set; }
        public string? CompanyName { get; set; }
        public string? CustomerName { get; set; }
    }
}
