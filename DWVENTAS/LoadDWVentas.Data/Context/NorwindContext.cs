


using LoadDWVentas.Data.Entities.Northwind;
using LoadDWVentas.Data.Entities.Norwind;
using Microsoft.EntityFrameworkCore;

namespace LoadDWVentas.Data.Context
{
    public partial class NorwindContext : DbContext
    {
      

        public NorwindContext(DbContextOptions<NorwindContext> options) : base(options)
        {

        }
      
       

        #region"Db Sets"
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Shipper> Shippers { get; set; }
        public DbSet<Vwventa> Vwventas { get; set; }
        public DbSet<VwServedCustomer> VwServedCustomers { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VwServedCustomer>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("VW_ServedCustomers", "DWH");

               
            });

            modelBuilder.Entity<Vwventa>(entity =>
            {
                entity
                    .HasNoKey()
                    .ToView("VWVentas", "DWH");

             
            });
        }
    }
}
