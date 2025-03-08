using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.LinkModels;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Dynamic;
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
        private readonly IDataShaper<EmployeeDto> _dataShaper;
        private readonly IEmployeeLinks _employeeLinks;

        public EmployeeService(IRepositoryManager repositoryManager, ILoggerManager loggerManager, IMapper mapper, /*IDataShaper<EmployeeDto> dataShaper*/ IEmployeeLinks employeeLinks)
        {
            _repositoryManager = repositoryManager;
            _loggerManager = loggerManager;
            _mapper = mapper;
            _employeeLinks = employeeLinks;
        }

        public async Task<(/*IEnumerable<EmployeeDto>*/ /*IEnumerable<ShapedEntity> employees*/ 
            LinkResponse LinkResponse, MetaData metaData)> GetEmployees
            (Guid companyId, LinkParameters linkParameters, bool trackChanges)
        {
            if (!linkParameters.EmployeeParameters.ValidAgeRange)
                throw new MaxAgeRangeBadRequestException();

            await CheckIfCompanyExists(companyId, trackChanges);

            var employeesFromDbWithMetaData = 
                await _repositoryManager.Employee.GetEmployees(companyId, linkParameters.EmployeeParameters, trackChanges);
            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDbWithMetaData);

            // thường là sẽ trả về toàn bộ field data của employeesDto
            // 20.3 đã triển khai shape - chọn field muốn trả về từ employeeDto
            //var shapeData = _dataShaper.ShapeData(employeesDto, linkParameters.EmployeeParameters.Fields);
            // do datashape đã tồn tại bên trong _employeeLinks nên không cần thiết nữa

            var links = _employeeLinks.TryGenerateLinks(employeesDto, linkParameters.EmployeeParameters.Fields, companyId, linkParameters.HttpContent);

            return (LinkResponse: links, metaData: employeesFromDbWithMetaData.MetaData);
        }

        public async Task<ShapedEntity> GetEmployee(Guid companyId, Guid id, bool trackChanges)
        {
            await CheckIfCompanyExists(companyId, trackChanges);

            var employeeDb = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, trackChanges);

            var employee = _mapper.Map<ShapedEntity>(employeeDb);
            return employee;
        }

        public async Task<EmployeeDto> CreateEmployeeForCompany(Guid companyId, EmployeeForCreationDto employeeForCreation, bool trackChanges)
        {
            await CheckIfCompanyExists(companyId, trackChanges);

            var employeeEntity = _mapper.Map<Employee>(employeeForCreation);

            _repositoryManager.Employee.CreateEmployeeForComapny(companyId, employeeEntity);
            _repositoryManager.SaveAsync();

            var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);

            return employeeToReturn;
        }
        public async Task DeleteEmployeeForCompany(Guid companyId, Guid id, bool trackChanges)
        {
            await CheckIfCompanyExists(companyId, trackChanges);

            var employeeForCompany = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, trackChanges);

            if (employeeForCompany is null)
                throw new EmployeeNotFoundException(id);

            _repositoryManager.Employee.DeleteEmployee(employeeForCompany);
            _repositoryManager.SaveAsync();
        }

        // đây là cách cập nhật kết nối (chỉ càn update trong service)
        // cập nhật kết nối là vẫn truy cập qua data lấy ra sau đó chỉnh sửa cập nhật lại thay đổi trạng thái sau đó savechange
        //nếu update cùng 1 ef instant(ví dụ) thì đó là cập nhật kết nối
        //còn ngược lại nếu lấy data từ ef instant này rồi đừa qua 1 new ef instant khác là khác kết nối nên được gọi là cập nhật không kết nối
        public async Task UpdateEmployeeForCompany(Guid companyId, Guid id, EmployeeForUpdateDto employeeForUpdate, bool compTrackChanges, bool empTrackChages)
        {
            await CheckIfCompanyExists(companyId, compTrackChanges);

            // mục đích emptrackchnage true là để cập nhật trạng thái của 1 entity state = modified (thay đổi)
            // khi gọi savechange() ef core cần tạo lệnh để UPDATE thay đổi vào database
            // empTrackChages được cập nhật tại controller
            // employeeEntity này chỉ lấy ra từ data chứ không thay đổi gì
            var employeeEntity = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, empTrackChages);
            if (employeeEntity is null)
                throw new EmployeeNotFoundException(id);

            // việc map này giúp đẩy dữ liệu đã thay đổi từ client cho employeeForUpdate ánh xạ qua employeeEntity từ đó dẫn đến thay đổi giá trị
            _mapper.Map(employeeForUpdate, employeeEntity);
            _repositoryManager.SaveAsync();
        }

        // lấy cả cong ty lẫn nhân viên 
        public async Task<(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)> GetEmployeeForPatch(
            Guid companyId, Guid id, bool compTrackChanges, bool empTrachChanges)
        {
            await CheckIfCompanyExists(companyId, compTrackChanges);

            var employeeEntity = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, empTrachChanges);

            // cái này chuyển loại Employee sang EmployeeForUpdateDto
            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);

            return (employeeToPatch, employeeEntity);
        }

        public void SaveChangesForPatch(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)
        {
            _mapper.Map(employeeToPatch, employeeEntity);
            _repositoryManager.SaveAsync();
        }

        private async Task CheckIfCompanyExists(Guid companyId, bool trackChanges)
        {
            var company = await _repositoryManager.Company.GetCompany(companyId, trackChanges);
            if (company is null)
                throw new CompanyNotFoundException(companyId);
        }

        private async Task<Employee> GetEmployeeForCompanyAndCheckIfItExists(Guid companyId, Guid id, bool trackChanges)
        {
            var employeeEntity = await _repositoryManager.Employee.GetEmployee(companyId, id, trackChanges);
            if (employeeEntity is null)
                throw new EmployeeNotFoundException(id);

            return employeeEntity;
        }
    }
}
