using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Responses
{
    // dùng lớp này có kiểu trả về thành công
    public sealed class ApiOkResponse<TResult> : ApiBaseResponse
    {
        // TResult này là thuộc tính chung vì nó có thể chứa các loại kết quả trả về khác nhau
        public TResult Result { get; set; }
        public ApiOkResponse(TResult result) : base(true)
        {
            Result = result;
        }
    }
}
