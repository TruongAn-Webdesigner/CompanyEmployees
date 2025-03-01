using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    internal sealed class EmployeeService : IEmployeeService
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILoggerManager _loggerManager;
        private readonly IMapper _mapper;

        public EmployeeService(IRepositoryManager repositoryManager, ILoggerManager loggerManager, IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _loggerManager = loggerManager;
            _mapper = mapper;
        }

        public IEnumerable<EmployeeDto> GetEmployees(Guid companyId, bool trackChanges)
        {
            var company = _repositoryManager.Company.GetCompany(companyId, trackChanges);
            if (company is null)
                throw new CompanyNotFoundException(companyId);

            var employeesFromDb = _repositoryManager.Employee.GetEmployees(companyId, trackChanges);
            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);

            return employeesDto;
        }

        public EmployeeDto GetEmployee(Guid companyId, Guid id, bool trackChanges)
        {
            var company = _repositoryManager.Company.GetCompany(companyId, trackChanges);
            if (company is null)
                throw new CompanyNotFoundException(companyId);

            var employeeDb = _repositoryManager.Employee.GetEmployee(companyId, id, trackChanges);
            if (employeeDb is null)
                throw new EmployeeNotFoundException(id);

            var employee = _mapper.Map<EmployeeDto>(employeeDb);
            return employee;
        }

        public EmployeeDto CreateEmployeeForCompany(Guid companyId, EmployeeForCreationDto employeeForCreation, bool trackingChanges)
        {
            var company = _repositoryManager.Company.GetCompany(companyId, trackingChanges);
            if (company is null)
                throw new CompanyNotFoundException(companyId);

            var employeeEntity = _mapper.Map<Employee>(employeeForCreation);

            _repositoryManager.Employee.CreateEmployeeForComapny(companyId, employeeEntity);
            _repositoryManager.Save();

            var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);

            return employeeToReturn;
        }
        public void DeleteEmployeeForCompany(Guid companyId, Guid id, bool trackChanges)
        {
            var company = _repositoryManager.Company.GetCompany(companyId, trackChanges);
            if (company is null)
                throw new CompanyNotFoundException(companyId);

            var employeeForCompany = _repositoryManager.Employee.GetEmployee(companyId, id, trackChanges);

            if (employeeForCompany is null)
                throw new EmployeeNotFoundException(id);

            _repositoryManager.Employee.DeleteEmployee(employeeForCompany);
            _repositoryManager.Save();
        }

        // đây là cách cập nhật kết nối (chỉ càn update trong service)
        // cập nhật kết nối là vẫn truy cập qua data lấy ra sau đó chỉnh sửa cập nhật lại thay đổi trạng thái sau đó savechange
        //nếu update cùng 1 ef instant(ví dụ) thì đó là cập nhật kết nối
        //còn ngược lại nếu lấy data từ ef instant này rồi đừa qua 1 new ef instant khác là khác kết nối nên được gọi là cập nhật không kết nối
        public void UpdateEmployeeForCompany(Guid companyId, Guid id, EmployeeForUpdateDto employeeForUpdate, bool compTrackChanges, bool empTrackChages)
        {
            var company = _repositoryManager.Company.GetCompany(companyId, compTrackChanges);
            if (company is null)
                throw new CompanyNotFoundException(companyId);

            // mục đích emptrackchnage true là để cập nhật trạng thái của 1 entity state = modified (thay đổi)
            // khi gọi savechange() ef core cần tạo lệnh để UPDATE thay đổi vào database
            // empTrackChages được cập nhật tại controller
            // employeeEntity này chỉ lấy ra từ data chứ không thay đổi gì
            var employeeEntity = _repositoryManager.Employee.GetEmployee(companyId, id, empTrackChages);
            if (employeeEntity is null)
                throw new EmployeeNotFoundException(id);

            // việc map này giúp đẩy dữ liệu đã thay đổi từ client cho employeeForUpdate ánh xạ qua employeeEntity từ đó dẫn đến thay đổi giá trị
            _mapper.Map(employeeForUpdate, employeeEntity);
            _repositoryManager.Save();
        }

        // lấy cả cong ty lẫn nhân viên 
        public (EmployeeForUpdateDto employeeToPatch, Employee employeeEntity) GetEmployeeForPatch(
            Guid companyId, Guid id, bool compTrackChanges, bool empTrachChanges)
        {
            var company = _repositoryManager.Company.GetCompany(companyId, compTrackChanges);
            if (company is null)
                throw new CompanyNotFoundException(companyId);

            var employeeEntity = _repositoryManager.Employee.GetEmployee(companyId, id, empTrachChanges);
            if (employeeEntity is null)
                throw new EmployeeNotFoundException(id);

            // cái này chuyển loại Employee sang EmployeeForUpdateDto
            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);

            return (employeeToPatch, employeeEntity);
        }

        public void SaveChangesForPatch(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)
        {
            _mapper.Map(employeeToPatch, employeeEntity);
            _repositoryManager.Save();
        }
    }
}
