using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesAnalytics.Api.Data.Entities
{
    [Table("Clientes")]
    public class Customer
    {
        [Key]
        public int IdCliente { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        //public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
    }
}
