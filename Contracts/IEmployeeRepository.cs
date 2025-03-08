using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.RequestFeatures;

namespace Contracts
{
    public interface IEmployeeRepository
    {
        Task<PagedList<Employee>> GetEmployees(Guid companyId,
            EmployeeParameters employeeParameters, bool trackChanges);
        Task<Employee> GetEmployee(Guid companyId, Guid id, bool trackChanges);
        void CreateEmployeeForComapny(Guid companyId, Employee employee);
        void DeleteEmployee(Employee employee);
    }
}
