using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Exceptions
{
    public sealed class CompanyNotFoundException : NotFoundException
    {
        public CompanyNotFoundException(Guid companyId)
            :base ($"The company with id: {companyId} doesn't exist in the database.") // đoạn text này sẽ được thrrow ra nếu dữ liệu null trong postman
        { }
    }
}
