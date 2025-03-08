using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RequestFeatures
{
    // chứa tham số RequestParameters
    public class EmployeeParameters : RequestParameters
    {
        public EmployeeParameters() => OrderBy = "name";
        public uint MinAge { get; set; }
        public uint MaxAge { get; set; } = int.MaxValue;

        // đây là validate cơ bản khi max age có lớn hơn min không, nếu không thì báo lỗi cho user
        public bool ValidAgeRange => MaxAge > MinAge;

        // 18.2 mở rông tính năng search
        public string? SearchTerm { get; set; }

    }
}
