using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesAnalytics.Api.Data.Entities
{
    [Table("Productos")]
    public class Product
    {
        [Key]
        public int IdProducto { get; set; }

        public string NombreProducto { get; set; }

        public string Categoria { get; set; }

        public decimal Precio { get; set; }

        public int Stock { get; set; }

        //public virtual ICollection<DetallesVentum> DetallesVenta { get; set; } = new List<DetallesVentum>();
    }
}
