
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

        //public virtual ICollection<DetallesVentum> DetallesVenta { get; set; } = new List<DetallesVentum>();

        //public virtual Cliente IdClienteNavigation { get; set; }
    }
}
