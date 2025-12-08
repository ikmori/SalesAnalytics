using Microsoft.EntityFrameworkCore;
using SalesAnalytics.Domain.Entities.Csv;
using SalesAnalytics.Domain.Entities.Db;

namespace SalesAnalytics.Persistence.Repositories.Db.Context
{
    public class SalesContext : DbContext
    {
        public SalesContext(DbContextOptions<SalesContext> options) : base(options)
        {
        }

        public DbSet<sale> Sales { get; set; }
        public DbSet<orderDetails> orderDetails { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<sale>(entity =>
            {
                entity.ToTable("Ventas");
                entity.HasKey(e => e.IdVenta);
            });

            modelBuilder.Entity<orderDetails>(entity =>
            {
                entity.ToTable("DetallesVenta");
                entity.HasKey(e => e.IdDetalleVenta);

                entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.TotalLinea).HasColumnType("decimal(18, 2)");

                
                entity.Ignore("IdProductoNavigation");
            });


        }
    }
}