using ClassLibrary2.Models;
using Microsoft.EntityFrameworkCore;
using SalesAnalytics.Domain.Entities.Dwh.Dimensions;

namespace SalesAnalytics.Persistence.Repositories.Dwh.Context
{
    public class SalesDwhContext : DbContext
    {
        public SalesDwhContext(DbContextOptions<SalesDwhContext> options) : base(options)
        {
        }

        public DbSet<DimCustomer> DimCustomers { get; set; }
        public DbSet<DimProduct> DimProducts { get; set; }
        public DbSet<DimStatus> DimStatuses { get; set; }
        public DbSet<DimDate> DimDates { get; set; }
        public DbSet<FactSale> FactSales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mapeo explícito a los esquemas
            modelBuilder.Entity<DimCustomer>().ToTable("DimCustomer", "Dimension");
            modelBuilder.Entity<DimProduct>().ToTable("DimProduct", "Dimension");
            modelBuilder.Entity<DimStatus>().ToTable("DimStatus", "Dimension");
            modelBuilder.Entity<DimDate>().ToTable("DimDate", "Dimension");
            modelBuilder.Entity<FactSale>().ToTable("FactSales", "Fact");
        }
    }
}