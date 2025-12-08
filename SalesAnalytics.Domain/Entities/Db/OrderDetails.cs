using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SalesAnalytics.Domain.Entities.Db
{
    [Table("DetallesVentum")]
    public class orderDetails
    {
        [Key]
        public int IdDetalleVenta { get; set; }

        public int IdVenta { get; set; }

        public int IdProducto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }
        [Column("TotalLinea")]
        public decimal TotalLinea { get; set; }

        public virtual Customer IdProductoNavigation { get; set; }
        [ForeignKey("IdVenta")]
        public virtual sale IdVentaNavigation { get; set; }
    }
}