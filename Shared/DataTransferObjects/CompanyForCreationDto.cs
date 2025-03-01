using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects
{
    // 9.4 mở rộng class thêm IEnumerable<EmployeeForCreationDto> Employees giúp tạo công ty + nhân viên cùng 1 request
    public record CompanyForCreationDto
    {
        [Required(ErrorMessage = "Company name is a required field.")]
        [MaxLength(30, ErrorMessage = "Maximum length for the Name is 30 characters.")]
        public string? Name { get; init; }

        [Required(ErrorMessage = "Address is a required field.")]
        public string? Address { get; init; }

        [Required(ErrorMessage = "Country is a required field.")]
        public string? Country { get; init; }

        public IEnumerable<EmployeeForCreationDto>? Employees { get; init; }
    }
}
