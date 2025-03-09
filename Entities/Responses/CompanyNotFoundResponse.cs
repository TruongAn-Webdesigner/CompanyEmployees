using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Responses
{
    public sealed class CompanyNotFoundResponse : ApiNotFoundResponse
    {
        // nó tạo 1 id thông báo gửi đến lớp cơ sở (sealed)
        public CompanyNotFoundResponse(Guid id)
            : base($"Company with id: {id} is not found in db.")
        {

        }
    }
}
