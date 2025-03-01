using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Exceptions
{
    // đây là lớp abstract sẽ là lớp cơ sở cho tất cả các trường hợp exception not found
    // lớp cơ sở này có lợi ích việc nhiều lớp khác kế thừa NotFoundException
    public abstract class NotFoundException : Exception
    {
        protected NotFoundException(string message)
            : base(message)
        { }
    }
}
