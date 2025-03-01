using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Contracts
{
    // nơi đinh nghĩa contract của service, bên ngoài sẽ gọi trực tiếp qua chứ không được qua class implemet service manager
    // - Presentation Layer gọi các class manager (Service layer) này có nhiệm vụ tổng hợp khai báo các dữ liệu cho Presentation Layer
    // - Sevice layer điều khiển Infrastructure Layer để truy cạp dữ liệu
    // - Service Layer(với các Service Classes) là lớp chịu trách nhiệm quản lý và điều phối các lớp khác để thực hiện nghiệp vụ
    // Service Classes sẽ quản lý các interface cần thiết cho Presentation
    public interface IServiceManager
    {
        ICompanyService CompanyService { get; }
        IEmployeeService EmployeeService { get; }
    }
}
