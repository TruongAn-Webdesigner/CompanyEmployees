using AutoMapper;
using Entities.Models;
using Shared.DataTransferObjects;

namespace CompanyEmployees
{
    public class MappingProfile : Profile
    {
        // package này hỗ trợ mapping, giảm việc mapping bằng code thủ công 
        public MappingProfile()
        {
            CreateMap<Company, CompanyDto>()
                // lỗi Internal server error nếu dùng ForMember, vì FullAddress nó không thể tìm thấy
                // nên dùng ForCtorParam và vì các đối tượng đang truyền tham số thông qua constructor
                .ForMember(c => c.FullAddress,
                    opt => opt.MapFrom(x => string.Join(' ', x.Address, x.Country)));
                //.ForCtorParam("FullAddress",
                //        opt => opt.MapFrom(x => string.Join(' ', x.Address, x.Country)));

            CreateMap<Employee, EmployeeDto>();

            CreateMap<CompanyForCreationDto, Company>();

            CreateMap<EmployeeForCreationDto, Employee>();

            // mặc định map sẽ là ánh xạ 1 chiều, nếu dùng ReverseMap nó sẽ tự đôngj ánh xạ thêm 1 chiều ngược lại nữa
            CreateMap<EmployeeForUpdateDto, Employee>().ReverseMap();

            CreateMap<CompanyForUpdateDto, Company>();
        }
    }
}
