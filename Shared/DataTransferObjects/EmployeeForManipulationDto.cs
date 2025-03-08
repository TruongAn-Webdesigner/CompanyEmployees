using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects
{
    // class này cấu hình validate chung cho các class kế thừa khác
    public abstract record EmployeeForManipulationDto
    {
        [Required(ErrorMessage = "Employee name is a required field.")]
        [MaxLength(30, ErrorMessage = "Maximum length for the Name is 30 characters.")]
        public string? Name { get; init; }

        [Required(ErrorMessage = "Age is a required field.")]
        [Range(18, int.MaxValue, ErrorMessage = "Age is required and it can't be lower than 18.")]
        // vì int mặc định là 0 nên khi không điền nó cũng hiểu là 0 và không có message validate nào hiển thị, nên đăt range là cần thiết
        public int? Age { get; init; }

        [Required(ErrorMessage = "Position is a required field.")]
        [MaxLength(20, ErrorMessage = "Maximum length for the Position is 20 characters.")]
        public string? Position { get; init; }
    }
}
