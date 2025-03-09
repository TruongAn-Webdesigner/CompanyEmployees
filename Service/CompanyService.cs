using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Entities.Responses;
using Service.Contracts;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    // đây sẽ là nơi gọi các phương thức từ repository class (CompanyRepository hoặc EmployeeRepository)
    internal sealed class CompanyService : ICompanyService
    {
        // đây là inject phòng khi không nhớ
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILoggerManager _loggerManager;
        private readonly IMapper _mapper;

        public CompanyService(IRepositoryManager repositoryManager, ILoggerManager loggerManager, IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _loggerManager = loggerManager;
            _mapper = mapper;
        }
        // 32.2 IEnumerable<CompanyDto> -> ApiBaseResponse
        public async Task<ApiBaseResponse> GetAllCompanies(bool trackChanges)
        {
            //try => lọa bỏ try catch vì bây giờ đã có ExceptionMiddlewareExtensions xử lý exception rồi
            //{
                var companies = await _repositoryManager.Company.GetAllCompanies(trackChanges);

                //khi không dùng mapper
                //var companiesDto = companies.Select(c => new CompanyDto(c.Id, c.Name ?? "", string.Join(' ', c.Address, c.Country))).ToList();

                // sau khi dùng mapper
                var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);

                return new ApiOkResponse<IEnumerable<CompanyDto>>(companiesDto);
            //}
            //catch (Exception ex)
            //{
            //    _loggerManager.LogError($"Something went wrong in the {nameof(GetAllCompanies)} service method {ex}");
            //    throw;
            //}
        }
        // lấy đơn dữ liệu 
        public async Task<ApiBaseResponse> GetCompany(Guid id, bool trackChanges)
        {
            // 15.6 không cần dùng
            //var company = await _repositoryManager.Company.GetCompany(id, trackChanges);
            //if (company is null)
            //    throw new CompanyNotFoundException(id); // gọi đến sealed class CompanyNotFoundException
            var company = await GetCompanyAndCheckIfItExists(id, trackChanges);

            var companyDto = _mapper.Map<CompanyDto>(company);
            return new ApiOkResponse<CompanyDto>(companyDto);
        }

        public async Task<CompanyDto> CreateCompany(CompanyForCreationDto company)
        {
            var companyEntity = _mapper.Map<Company>(company);

            _repositoryManager.Company.CreateCompany(companyEntity);
            await _repositoryManager.SaveAsync();

            var companyToReturn = _mapper.Map<CompanyDto>(companyEntity);

            return companyToReturn;
        }

        // lấy nhiều company theo (guid id, guid id,...)
        public async Task<IEnumerable<CompanyDto>> GetByIds(IEnumerable<Guid> ids, bool trackChanges)
        {
            if (ids is null)
                throw new IdParametersBadRequestException();
            // do cách phân tách giữa client và service của DTO nên khi gọi thì qua models nhưng trả về thì qua DTO
            var companyEntites = await _repositoryManager.Company.GetByIds(ids, trackChanges);
            if (ids.Count() != companyEntites.Count())
                throw new CollectionByIdsBadRequestException();

            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntites);

            return companiesToReturn;
        }

        // Tạo nhiều company trong cùng 1 request
        public async Task<(IEnumerable<CompanyDto> companies, string ids)> CreateCompanyCollection(IEnumerable<CompanyForCreationDto> companyCollection)
        {
            if (companyCollection is null)
                throw new CompanyCollectionBadRequest();

            var companyEntites = _mapper.Map<IEnumerable<Company>>(companyCollection);
            foreach (var company in companyEntites)
            {
                _repositoryManager.Company.CreateCompany(company);
            }

            await _repositoryManager.SaveAsync();

            var companyCollectionToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companyEntites);
            var ids = string.Join(",", companyCollectionToReturn.Select(c => c.Id));

            return (companies: companyCollectionToReturn, ids: ids);
        }

        public async Task DeleteCompany(Guid companyId, bool trackChanges)
        {
            //var company = await _repositoryManager.Company.GetCompany(companyId, trackChanges);
            //if (company is null)
            //    throw new CompanyNotFoundException(companyId);

            var company = await GetCompanyAndCheckIfItExists(companyId, trackChanges);

            _repositoryManager.Company.DeleteCompany(company);
            await _repositoryManager.SaveAsync();
        }

        public async Task UpdateCompany(Guid companyId, CompanyForUpdateDto companyForUpdate, bool trackChanges)
        {
            //var companyEntity = await _repositoryManager.Company.GetCompany(companyId, trackChanges);
            //if (companyEntity is null)
            //    throw new CompanyNotFoundException(companyId);

            var company = await GetCompanyAndCheckIfItExists(companyId, trackChanges);

            _mapper.Map(companyForUpdate, company);
            await _repositoryManager.SaveAsync();
        }

        // hàm mở rộng gọi chung hạn chế lặp code
        private async Task<Company> GetCompanyAndCheckIfItExists(Guid id, bool trackChanges)
        {
            var company = await _repositoryManager.Company.GetCompany(id, trackChanges);
            if (company is null)
                throw new CompanyNotFoundException(id);

            return company;
        }
    }
}
