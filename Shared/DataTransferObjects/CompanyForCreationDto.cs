using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects
{
    // 9.4 mở rộng class thêm IEnumerable<EmployeeForCreationDto> Employees giúp tạo công ty + nhân viên cùng 1 request
    public record CompanyForCreationDto(string Name, string Address, string Country, IEnumerable<EmployeeForCreationDto> Employees);
}
