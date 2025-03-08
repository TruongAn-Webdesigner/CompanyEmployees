using Entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Repository.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class RepositoryContext : /*DbContext*/ IdentityDbContext<User> // 27.1 triển khai identity
    {
        public RepositoryContext(DbContextOptions options) : base(options)
        {
        }

        // sau khi tạo dữ liệu import cần phải gọi cấu hình đó để khởi chạy Add-Migration InitiaData -> Update-Database
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 27.1 đẻ migration hoạt động cần gọi tới cái này
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new CompanyConfiguaration());
            modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
        }

        public DbSet<Company>? Companies { get; set; }
        public DbSet<Employee>? Employees { get; set; }
    }
}
