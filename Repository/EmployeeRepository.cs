using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Repository.Extensions;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    // Repository user interface sau khi đã thực hiện Repository pattern logic
    // bằng cách kế thừa kiểu T này có thể dùng logic có trong RepositoryBase<T>
    public class EmployeeRepository : RepositoryBase<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }

        public async Task<PagedList<Employee>> GetEmployees(Guid companyId,
            EmployeeParameters employeeParameters, bool trackChanges)
        {
            //17.3 thêm filter cơ bản -> nanga cao
            var employees = await FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges)
            //18.2 filter theo age nâng cao
            .FilterEmployees(employeeParameters.MinAge, employeeParameters.MaxAge)
            //18.2 search
            .Search(employeeParameters.SearchTerm)
            //19.2 sort
            .Sort(employeeParameters.OrderBy)
            .OrderBy(e => e.Name)
            .ToListAsync(); // đối với dữ liệu dài hàng triệu dòng thì giải pháp này không nhanh bằng thêm await count() 16.4.1 Additional Advice
            // 16.4 thêm paging
            return PagedList<Employee>.ToPagedList(employees, employeeParameters.PageNumber, employeeParameters.PageSize);
        }
            

        public async Task<Employee> GetEmployee(Guid companyId, Guid id, bool trackChanges) =>
            await FindByCondition(e => e.CompanyId.Equals(companyId) && e.Id.Equals(id), trackChanges)
            .SingleOrDefaultAsync();

        public void CreateEmployeeForComapny(Guid companyId, Employee employee)
        {
            employee.CompanyId = companyId;
            Create(employee);
        }

        public void DeleteEmployee(Employee employee)
        {
            Delete(employee);
        }
    }
}
