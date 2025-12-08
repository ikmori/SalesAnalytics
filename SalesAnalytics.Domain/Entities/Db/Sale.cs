
using System.ComponentModel.DataAnnotations.Schema;
namespace SalesAnalytics.Domain.Entities.Db
{
    [Table("Ventas")]
    public class sale
    {
        public int IdVenta { get; set; }

        public int IdCliente { get; set; }

        public DateTime FechaVenta { get; set; }

        public string Status { get; set; }

        //[ForeignKey("IdVenta")]

        public virtual ICollection<orderDetails> orderDetails { get; set; } = new List<orderDetails>();
        [ForeignKey("IdCliente")]
        public virtual Customer IdClienteNavigation { get; set; }
    }
}
