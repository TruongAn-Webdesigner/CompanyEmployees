using AutoMapper;
using Contracts;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    // nơi triển khai service, vì hiện tại đây là nơi mà bên ngoài gọi vào nhưng vì cấu trúc onion nên cần 1 Service.Contracts interface
    // bên ngoài sẽ gọi gián tiếp qua chứ không được trực tiếp qua class này.
    // - Presentation Layer gọi các class manager (Service layer) này có nhiệm vụ tổng hợp khai báo các dữ liệu cho Presentation Layer
    // - Sevice layer điều khiển Infrastructure Layer để truy cạp dữ liệu
    // - Service Layer(với các Service Classes) là lớp chịu trách nhiệm quản lý và điều phối các lớp khác để thực hiện nghiệp vụ
    // Service Classes sẽ quản lý các interface cần thiết cho Presentation
    public sealed class ServiceManager : IServiceManager
    {
        private readonly Lazy<ICompanyService> _companyService;
        private readonly Lazy<IEmployeeService> _employeeService;
        
        public ServiceManager(IRepositoryManager repositoryManager, ILoggerManager loggerManager , IMapper mapper)
        {
            _companyService = new Lazy<ICompanyService>(() => new CompanyService(repositoryManager, loggerManager, mapper));
            _employeeService = new Lazy<IEmployeeService>(() => new EmployeeService(repositoryManager, loggerManager, mapper));
        }

        public ICompanyService CompanyService => _companyService.Value;

        public IEmployeeService EmployeeService => _employeeService.Value;
    }
}
