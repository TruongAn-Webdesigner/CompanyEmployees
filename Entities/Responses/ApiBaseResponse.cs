using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Responses
{
    // lớp abstract này kiểu trả về chính cho toàn bộ phương thức, trả về thành công hoặc lỗi
    public abstract class ApiBaseResponse
    {
        // success cho biết action đó có thành công hay không
        public bool Success { get; set; }
        protected ApiBaseResponse(bool success)
        {
            Success = success;
        }
    }
}
