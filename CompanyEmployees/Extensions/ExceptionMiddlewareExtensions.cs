using Contracts;
using Entities.ErrorModel;
using Entities.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace CompanyEmployees.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(this WebApplication app, ILoggerManager logger)
        {
            // cái use này sẽ thêm middleware vào pipeline như bên ngoài program để bắt exception
            app.UseExceptionHandler(appError =>
            // ở đây dùng appError biến của IApplicationBuilder, sau đó gọi Run nó sẽ thêm middleware vào đầu cuối chương trình ứng dụng
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if(contextFeature != null)
                    {
                        // thay vì dùng status code cứng thì mình có thể điền nó dựa trên ngoại lệ mà service trả về
                        context.Response.StatusCode = contextFeature.Error switch
                        {
                            // NotFoundException lấy từ abstract class NotFoundException
                            NotFoundException => StatusCodes.Status404NotFound,
                            BadRequestException => StatusCodes.Status400BadRequest,
                            _ => StatusCodes.Status500InternalServerError
                        };
                        logger.LogError($"Something went wrong: {contextFeature.Error}");

                        await context.Response.WriteAsync(new ErrorDetails()
                        {
                            StatusCode = context.Response.StatusCode,
                            // thay vì tự viết thì có thể dựa vào thuộc tính Message của ErrorDetails trả dưới dạng phản hồi, contextFeature.Error.Message sẽ dựa vào 
                            // chuỗi đã tạo sẵn từ CompanyNotFoundException
                            Message = contextFeature.Error.Message,
                            //Message = "Internal Server Error.",
                        }.ToString());
                    }
                }));
        }
    }
}
