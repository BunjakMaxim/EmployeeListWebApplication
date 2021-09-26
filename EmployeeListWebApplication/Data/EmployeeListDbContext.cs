using EmployeeListWebApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeListWebApplication.Data
{
	public class EmployeeListDbContext : DbContext
    {
        public DbSet<Employee> Employees { get; protected set; }

        public EmployeeListDbContext(DbContextOptions<EmployeeListDbContext> options)
            : base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasIndex(e => e.PersonnelNumber)
                .IsUnique()
                .HasFilter("[IsStaff] = 1");
        }
    }
}
