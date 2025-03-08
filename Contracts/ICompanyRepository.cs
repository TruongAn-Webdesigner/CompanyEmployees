using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface ICompanyRepository
    {
        //14.3 chuyeển sang async
        Task<IEnumerable<Company>> GetAllCompanies(bool trackChanges);
        Task<Company> GetCompany(Guid companyId, bool trackChanges);
        // hàm này không đổi async bởi vì nó không làm thay đổi gì trong database, nó chỉ that đổi trạng thái của entity added và deleted
        void CreateCompany(Company company);
        Task<IEnumerable<Company>> GetByIds(IEnumerable<Guid> ids, bool trackChanges);
        void DeleteCompany(Company company);
    }
}
