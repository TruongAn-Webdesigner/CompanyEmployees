using AutoMapper;
using Contracts;
using Entities.ConfigurationModels;
using Microsoft.Extensions.Options;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Service.Contracts;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    // nơi triển khai service, vì hiện tại đây là nơi mà bên ngoài gọi vào nhưng vì cấu trúc onion nên cần 1 Service.Contracts interface
    // bên ngoài sẽ gọi gián tiếp qua chứ không được trực tiếp qua class này.
    // - Presentation Layer gọi các class manager (Service layer) này có nhiệm vụ gửi và nhận các request Sevice Layer
    // - Sevice layer điều khiển Infrastructure Layer để truy cạp dữ liệu
    // - Service Layer(với các Service Classes) là lớp chịu trách nhiệm quản lý và điều phối các lớp khác để thực hiện nghiệp vụ
    // Service Classes sẽ quản lý các interface cần thiết cho Presentation
    public sealed class ServiceManager : IServiceManager
    {
        private readonly Lazy<ICompanyService> _companyService;
        private readonly Lazy<IEmployeeService> _employeeService;
        private readonly Lazy<IAuthenticationService> _authenticationService;
        
        public ServiceManager(
            IRepositoryManager repositoryManager,
            ILoggerManager loggerManager ,
            IMapper mapper,
            //IDataShaper<EmployeeDto> dataShaper, 21.5 không cần nữa do đã khai bào trong IEmployeeLinks
            IEmployeeLinks employeeLinks,
            UserManager<User> userManager,
            /*IConfiguration configuration do dùng IOptions nên không cần thiết*/
            //IOptions<JwtConfiguration> configuration  //29.2.1 
            IOptionsSnapshot<JwtConfiguration> configuration // 29.2.2 reload lại cấu hình mà không dừng app
            )
        {
            _companyService = new Lazy<ICompanyService>(() => new CompanyService(repositoryManager, loggerManager, mapper));
            _employeeService = new Lazy<IEmployeeService>(() => new EmployeeService(repositoryManager, loggerManager, mapper, employeeLinks));
            _authenticationService = new Lazy<IAuthenticationService>(() =>
            new AuthenticationService(loggerManager, mapper, userManager, configuration));
        }

        public ICompanyService CompanyService => _companyService.Value;

        public IEmployeeService EmployeeService => _employeeService.Value;

        public IAuthenticationService AuthenticationService => _authenticationService.Value;
    }
}
