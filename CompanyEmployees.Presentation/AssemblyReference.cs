using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyEmployees.Presentation
{
    // đây là controller, vì controller của main project có thể phá vỡ qui tác onion vì nó có thể dùng trực tiếp các class implement
    // do main project đã được reference nên nó cũng có quyền được nhúng và dùng
    // nên việc tách để hạn chế việc trên.
    public static class AssemblyReference
    {
    }
}
