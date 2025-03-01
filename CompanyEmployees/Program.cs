using CompanyEmployees.Extensions;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using NLog;

var builder = WebApplication.CreateBuilder(args);

LogManager.Setup().LoadConfigurationFromFile(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

//dùng newtonjson thay cho sys.text.json
// phần này cấu hình hỗ trợ json patch
NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter() =>
    new ServiceCollection().AddLogging().AddMvc().AddNewtonsoftJson()
    .Services.BuildServiceProvider()
    .GetRequiredService<IOptions<MvcOptions>>().Value.InputFormatters
    .OfType<NewtonsoftJsonPatchInputFormatter>().First();

// Add services to the container.
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntergration();
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    // nếu dùng thuộc tính này thì nó ghi đè lên validate mặc định được triển khai theo [ApiController]
    // và có thể giúp debug 
    options.SuppressModelStateInvalidFilter = true;
});

// dòng code này chính là khai báo IoC cho controller, không có nó thì api không hoạt động
builder.Services.AddControllers(config =>
{
    // chuyển kiểu trả về dữ liệu từ json -> xml
    config.RespectBrowserAcceptHeader = true;
    // giúp trả về 406 nếu loại phương tiện (media) máy chủ không hỗ trợ
    config.ReturnHttpNotAcceptable = true;
    // chèn 1 input vào đầu danh sách header Content-Type: application/json-patch+json
    config.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
}).AddXmlDataContractSerializerFormatters()
    .AddCustomCSVFormatter() //thêm định dạng trả về custom csv
    // khai báo vị trí controller custom mới
    .AddApplicationPart(typeof(CompanyEmployees.Presentation.AssemblyReference).Assembly);

// tạo biến ứng dụng loại WebApplication
// nó rất quan trọng vì WebApplication sẽ giúp build (xây dựng và chạy) các middleware bên dưới hoạt động bằng IApplicationBuilder
var app = builder.Build();

var logger = app.Services.GetRequiredService<ILoggerManager>();
app.ConfigureExceptionHandler(logger);

if (app.Environment.IsProduction())
    app.UseHsts();

// đoạn này cần xóa đi vì không cần bây giờ
// Configure the HTTP request pipeline.
// Client (IP 203...) -> internet -> proxy server (IP 192...) -> ứng dụng
//if (app.Environment.IsDevelopment())
//    app.UseDeveloperExceptionPage();
//else
//    // Strict-Transport-Security header là 1 middleware dạng bảo mật
//    // giúp trình duyệt luôn luôn kết nối đến HTTPS ngay cả khi user gõ HTTP
//    // nó hoạt động trực tiếp trên ứng dụng chứ không phải proxy server
//    app.UseHsts();
// đoạn này cần xóa đi vì không cần bây giờ

app.UseHttpsRedirection(); // thêm chuyển hướng an toàn toàn từ HTTP sang HTTPS

// là middlware giúp cho phép thêm các tệp tĩnh: jpg, js, css,...
// các tệp này không phải là từ Client (IP 203...) mà chính là tệp mình tạo ra
// mặc định không có thì là wwwroot
app.UseStaticFiles();

// giúp chuyển tiếp các thông tin từ HTTP headers đặc biệt do proxy server (IIS) gửi đến vào trong REQUEST hiện tại của ứng dụng
// app.UseForwardedHeaders là middlware giúp thêm các thông tin (X-Forwarded-For) từ Client vào REQUEST, nếu không có thì REQUEST sẽ không có thông tin của client
// proxy server sẽ là nơi thêm các middlware này vào REQUEST
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All
});

app.UseCors("CorsPolicy");

// thực hiện xác thực ủy quyền truy cập ( dữạ trên  ví dụ: cookies, headers) trong request.
// nó sẽ kiểm tra end point xem mình có đủ quyền truy cập page hay không
// nó sẽ chỉ xác thực xem có mình có đủ quyền chứ không có làm gì khác nữa
// nếu xác thực false thì -> 403
app.UseAuthorization();

// nếu 1 request truy cập tồn tại chuỗi liệt kê thì trả về response đó, mục đích mở rộng hàm Run()
app.MapWhen(context => context.Request.Query.ContainsKey("testquerystring"), builder =>
{
    builder.Run(async context =>
        await context.Response.WriteAsync("Hello from MapWhen branch")
    );
});

// hàm use này giúp tạo ra nhiều request delegate hơn thay vì là 1
// thứ tự chạy: log 1 -> next qua Run() -> log 1 của Run() -> trả Response -> trả về tiếp tuc thực thi Use() chạy tiếp log 2 và kết thúc
// lưu ý có 2 request là do trình duyệt tự gửi 1 request nữa cho fva.icon, cái con lại là  /weatherforecast
//app.Use(async (context, next) =>
//{
//    Console.WriteLine($"Logic before executing the next delegate in the Use method");
//    await next.Invoke(); // bắt buộc phải có next
//    Console.WriteLine($"Logic after executing the next delegate in the Use method");
//    //context.Response.StatusCode = 200; //lưu ý nếu dùng next.Invoke() sẽ gây ra lỗi nếu thiết lập status code hoặc header vì response đã hoàn tất không được sửa
//});

//// app.Run là terminal middleware kết thúc middleware vì không có gọi next nào
//app.Run(async context =>
//{
//    Console.WriteLine($"Writing the response to the client in the Run method");
    
//    //await context.Response.WriteAsync("Hello");
//    await context.Response.WriteAsync($"Completed Response");
//});

// thêm end point từ controller actions vào IEndpointRouteBuilder (của Build())
// nếu vượt qua ủy quyền thì middleware này thực thi đến routing/endpoint 
app.MapControllers();

app.Run();
