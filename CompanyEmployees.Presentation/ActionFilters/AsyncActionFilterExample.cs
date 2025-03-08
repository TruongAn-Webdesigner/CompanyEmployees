using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyEmployees.Presentation.ActionFilters
{
    public class AsyncActionFilterExample : IAsyncActionFilter
    {
        // đẻe dùng được thì cần add vào IoC
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Console.WriteLine($"Logic before executing the next delegate in the Use method");
            var resulr = await next(); // bắt buộc phải có next
            Console.WriteLine($"Logic after executing the next delegate in the Use method");
        }
    }
}
