using Contracts;
using LoggerService;
using Microsoft.EntityFrameworkCore;
using Repository;
using Service;
using Service.Contracts;

namespace CompanyEmployees.Extensions
{
    // lớp mở rộng (extension method)
    // lớp mở rộng phải là static class (lớp tĩnh)
    public static class ServiceExtensions
    {
        // CORS (cross-origin resource sharing) chia sẻ tài nguyên đa nguồn
        // cấu hình này cho phép các domains khác được phép truy cập hay không vào ứng dụng của mình
        // cấu hình này là bắt buộc nếu muốn gửi request từ domain khác vào ứng dụng mình
        // vì chính sách Same-Origin Policy (Chính sách đồng nguồn gốc) bảo mật của trình duyệt chặn các truy cập khác nên cần cấu hình để nới lỏng qui tắc này
        public static void ConfigureCors(this IServiceCollection services) =>
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                builder.AllowAnyOrigin() // cho phép bất cứ nguồn truy cập nào, dùng WithOrigins("https...") để chỉ cho phép địa chỉ này.
                .AllowAnyMethod() // cho phép bất cứ phương thức nào (GET, POST,...), dùng WithMethods("POST", "GET") chỉ cho phép các phương thức liệt kê.
                .AllowAnyHeader()); // ... header nào (Content-Type, Authorization), dùng WithHeaders() ...
            });
        // mặc định asp .net sẽ tự chạy máy chủ riêng của nó (seft-hosted) nghĩa là trên máy tính chính mình (chạy bằng kestrel)
        // cấu hình IIS này giúp tiếp nhận request ngoài đưa vào seft-hosted (kestrel) -> kestrel xử lý trả về IIS -> ISS chuyển request về trình duyệt
        public static void ConfigureIISIntergration(this IServiceCollection services) =>
            services.Configure<IISOptions>(options =>
            {
                // để không là mặc định
            });
        // khai báo cấu hình log nhật ký
        public static void ConfigureLoggerService(this IServiceCollection services) =>
            services.AddSingleton<ILoggerManager, LoggerManager>();

        public static void ConfigureRepositoryManager(this IServiceCollection services) =>
            services.AddScoped<IRepositoryManager, RepositoryManager>();

        public static void ConfigureServiceManager(this IServiceCollection services) =>
            services.AddScoped<IServiceManager, ServiceManager>();

        public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration) =>
            services.AddDbContext<RepositoryContext>(opts =>
                opts.UseNpgsql(configuration.GetConnectionString("postgresSqlConnection")));

        public static IMvcBuilder AddCustomCSVFormatter(this IMvcBuilder builder) =>
            builder.AddMvcOptions(config => config.OutputFormatters.Add(new CsvOutputFormatter()));
    }

}
