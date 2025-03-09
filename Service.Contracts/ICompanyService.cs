using Entities.Models;
using Entities.Responses;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Contracts
{
    public interface ICompanyService
    {
        // 14.5 thêm async
        // ban đầu tham chiếu trực tiếp đến model Company nhưng do DTO nên tách ra giao tiếp thông qua shared, shared sẽ gọi models
        Task<ApiBaseResponse> GetAllCompanies(bool trackChanges);
        Task<ApiBaseResponse> GetCompany(Guid companyId, bool trackChanges);
        Task<CompanyDto> CreateCompany(CompanyForCreationDto company);
        Task<IEnumerable<CompanyDto>> GetByIds(IEnumerable<Guid> ids, bool trackChanges);

        // phương thức này chấp nhận 1 tập hơp loại CompanyForCreationDto làm tham số rồi trả về 1 Tuple gồm 2 trương companies, ids
        Task<(IEnumerable<CompanyDto> companies, string ids)> CreateCompanyCollection(IEnumerable<CompanyForCreationDto> companyCollection);

        Task DeleteCompany(Guid companyId, bool trackChanges);
        Task UpdateCompany(Guid companyId, CompanyForUpdateDto companyForUpdate, bool trackChanges);
    }
}
