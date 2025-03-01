using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Repository;

namespace CompanyEmployees.ContextFactory
{
    public class RepositoryContextFactory : IDesignTimeDbContextFactory<RepositoryContext>
    {
        public RepositoryContext CreateDbContext(string[] agrs)
        {
            // cấu hình chỉ định file để phương thức GetConnectionString bên dưới có thể truy cập để kết nối cấu hình từ appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // sử dụng sql server
            var builder = new DbContextOptionsBuilder<RepositoryContext>()
                .UseNpgsql(configuration.GetConnectionString("postgresSqlConnection"),
                    b => b.MigrationsAssembly("CompanyEmployees"));

            // trả về 1 instance mới của lớp RepositoryContext
            return new RepositoryContext(builder.Options);
        }
    }
}
