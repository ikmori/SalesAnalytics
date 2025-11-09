using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesAnalytics.Domain.Entities.Api
{
    //[Table("Clientes")]
    public class Customer
    {
        //[Key]
        public int IdCliente { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

    }
}
