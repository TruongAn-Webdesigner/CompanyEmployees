using Contracts;
using LoggerService;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository;
using Service;
using Service.Contracts;
using Microsoft.AspNetCore.Mvc.Versioning;
using Repository.Configuration;
using CompanyEmployees.Presentation.Controllers;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Identity;
using Entities.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Entities.ConfigurationModels;
using System.Reflection;
using Microsoft.OpenApi.Models;

namespace CompanyEmployees.Extensions
{
    // lớp mở rộng (extension method)
    // lớp mở rộng phải là static class (lớp tĩnh)
    public static class ServiceExtensions
    {
        // CORS (cross-origin resource sharing) chia sẻ tài nguyên đa nguồn
        // là 1 cơ chế cấp hoặc cho phép truy cập hoặc hạn chế từ các nguồn truy cập khác nhau
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

        // 21.4.1 đăng ký tùy chọn media types
        public static void AddCustomMediaTypes(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(config =>
            {
                var systemTextJsonOutputFormatter = config.OutputFormatters
                .OfType<SystemTextJsonOutputFormatter>()?.FirstOrDefault();
                if (systemTextJsonOutputFormatter != null)
                {
                    systemTextJsonOutputFormatter.SupportedMediaTypes
                    .Add("application/vnd.codemaze.hateoas+json"); // đây là media custom

                    //23.1 triển khai root document
                    systemTextJsonOutputFormatter.SupportedMediaTypes
                    .Add("application/vnd.codemaze.apiroot+json");
                }

                var xmlOutputFormatter = config.OutputFormatters
                .OfType<XmlDataContractSerializerOutputFormatter>()?
                .FirstOrDefault();

                if (xmlOutputFormatter != null)
                {
                    xmlOutputFormatter.SupportedMediaTypes
                    .Add("application/vnd.codemaze.hateoas+xml");

                    //23.1 triển khai root document
                    xmlOutputFormatter.SupportedMediaTypes
                    .Add("application/vnd.codemaze.apiroot+json");
                }
            });
        }

        // 24.1 cấu hình version của api
        public static void ConfigureVersioning(this IServiceCollection services)
        {
            //AddApiVersioning này giúp thêm api versioning vào service
            services.AddApiVersioning(opt =>
            {
                opt.ReportApiVersions = true; // thêm phiên bản api vào header
                opt.AssumeDefaultVersionWhenUnspecified = true; // thực hiện chỉ định phiên bản defaut nếu client không gửi version
                //opt.DefaultApiVersion = new ApiVersion(1, 0); // đặt số lượng phiên bản mặc định
                opt.ApiVersionReader = new HeaderApiVersionReader("api-version");// 24.2 cấu hình gửi version qua http nếu không muốn sửa router
                //opt.ApiVersionReader = new QueryStringApiVersionReader("api-version");//24.2 caấu hình version gửi thông qua query

                //24.2.5 có thể tạo cấu hình version chung mà không cần khai báo [apiversion]
                opt.Conventions.Controller<CompaniesController>()
                .HasDeprecatedApiVersion(new ApiVersion(1, 0)); // hàm này Deprecated đánh dấu ver lỗi thời 
                opt.Conventions.Controller<CompaniesV2Controller>()
                .HasApiVersion(new ApiVersion(2, 0)); // hàm này đánh dấu ver mới
                
            });
        }

        // 25.3 triển khai cache-store
        public static void ConfigureResponseCaching(this IServiceCollection services) =>
            services.AddResponseCaching();

        // 25.6 triển khai validate cache
        public static void ConfigureHttpCacheHeaders(this IServiceCollection services) =>
            services.AddHttpCacheHeaders(
                //25.6.1 cấu hình validate cache global
                (expirationOpt) =>
                {
                    expirationOpt.MaxAge = 65;
                    expirationOpt.CacheLocation = Marvin.Cache.Headers.CacheLocation.Private; // chuyển sang private cache
                },
                (validationOpt) =>
                {
                    validationOpt.MustRevalidate = true;
                });

        // 26.1 rate limit
        public static void ConfigureRateLimitingOptions(this IServiceCollection services)
        {
            // phần rate limit rules
            var rateLimitRules = new List<RateLimitRule>
            {
                new RateLimitRule
                {
                    Endpoint = "*", // bất kỳ điểm cuối nào
                    Limit = 30, // 3 req
                    Period = "5m" // 5 phút
                }
            };

            services.Configure<IpRateLimitOptions>(opt =>
            {
                opt.GeneralRules = rateLimitRules;
            });
            // dùng singleton mục đích là để lưu các bộ đếm, giới hạn, cấu hình
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
        }

        public static void ConfigureIdentity (this IServiceCollection services)
        {
            var builder = services.AddIdentity<User, IdentityRole>(o =>
            {
                o.Password.RequireDigit = true;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 10;
                o.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<RepositoryContext>()
            .AddDefaultTokenProviders();
        }

        //27.6 cấu hình jwt
        public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
        {   //29.1 triển khai binding 
            //var jwtSettings = configuration.GetSection("JwtSettings");
            // bind ràng buộc trực tiếp jwtConfiguration.Section và ánh xạ giá trị đến các thuộc tính tương ứng trong JwtConfiguration class, sau đó chỉ cần dùng các thuộc tính trong class đó thày vì các giá trị gán cứng ["..."]
            var jwtConfiguration = new JwtConfiguration();
            configuration.Bind(jwtConfiguration.Section, jwtConfiguration);

            var secretKey = Environment.GetEnvironmentVariable("SECRET");
            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfiguration.ValidIssuer,
                    ValidAudience = jwtConfiguration.ValidAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });
        }

        //29.2.1 triển khai dùng IOptions
        public static void AddJwtConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            //services.Configure<JwtConfiguration>(configuration.GetSection("JwtSettings")); -> chỉ áp dung 1 cấu hình
            // nếu có nhiều cấu hình trong appsetting thì có thể trỏ đến 1 class cấu hình chung duy nhất
            services.Configure<JwtConfiguration>("JwtSettings", configuration.GetSection("JwtSettings"));
            //services.Configure<JwtConfiguration>("JwtAPI2Settings", configuration.GetSection("JwtAPI2Settings"));
        }
        
        //30.2 triển khai swagger
        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(s =>
            {
                s.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Code Maze Api",
                    Version = "v1",
                    Description = "CompanyEmployees Api by CodeMaze and by me :)", //30.4 Mở rộng cấu hình swagger
                    TermsOfService = new Uri("https://google.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "ABC Nguyễn",
                        Email = "abc@abc.com",
                        Url = new Uri("https://www.google.com"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "CompanyEmployees API LICX",
                        Url = new Uri("https://example.com/license"),
                    }
                });
                s.SwaggerDoc("v2", new OpenApiInfo { Title = "Core Maze Api", Version = "v2" });

                // 30.4 Bật xml comment cho từng hành động trong swagger
                var xmlFile = $"{typeof(Presentation.AssemblyReference).Assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                s.IncludeXmlComments(xmlPath);

                //30.3 triển khai Authorization cho swagger
                s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Place to add JWT with Bearer",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                s.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Name = "Bearer",
                        },
                        new List<string>()
                    }
                });
            });
        }
    }

}
