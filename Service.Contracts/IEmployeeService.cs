using Entities.LinkModels;
using Entities.Models;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Contracts
{
    public interface IEmployeeService
    {
        // 16 thêm paging EmployeeParameters
        // 20.4 ExpandoObject -> Entity
        // 21.3 Entity->ShapedEntity
        // 21.5 employeeParameters -> LinkParameters
        Task<(/*IEnumerable<EmployeeDto>*/ /*IEnumerable<ShapedEntity> employees*/
            LinkResponse LinkResponse, MetaData metaData)> GetEmployees
            (Guid companyId, /*EmployeeParameters employeeParameters*/ LinkParameters linkParameters, bool trackChanges);
        Task<ShapedEntity> GetEmployee(Guid companyId, Guid id, bool trackChanges);
        Task<EmployeeDto> CreateEmployeeForCompany(Guid companyId, 
            EmployeeForCreationDto employeeForCreation, bool trackingChanges);
        Task DeleteEmployeeForCompany(Guid companyId, Guid id, bool trackChanges);
        Task UpdateEmployeeForCompany(
            Guid companyId, Guid id, EmployeeForUpdateDto employeeForUpdate, bool compTrackChanges, bool empTrackChages);
        Task<(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)> GetEmployeeForPatch(
            Guid companyId, Guid id, bool compTrackChanges, bool empTrachChanges);
        void SaveChangesForPatch(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity);
    }
}
