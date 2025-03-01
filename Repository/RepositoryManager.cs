using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class RepositoryManager : IRepositoryManager
    {
        private readonly RepositoryContext _repositoryContext;
        // lazy nhằm trì hoãn khởi tạo đối tượng cho đến khi mình cần truy cập đến lần đầu, giúp tăng hiêuj suất khi khỏi động app
        private readonly Lazy<ICompanyRepository> _companyRepository;
        private readonly Lazy<IEmployeeRepository> _employeeRepository;
        public RepositoryManager(RepositoryContext repositoryContext)
        {
            _repositoryContext = repositoryContext;
            _companyRepository = new Lazy<ICompanyRepository>(() => new
CompanyRepository(repositoryContext));
            _employeeRepository = new Lazy<IEmployeeRepository>(() => new
            EmployeeRepository(repositoryContext));
        }

        // hàm này triển khai phương pháp từ Repository pattern, giờ đây CRUD chỉ cần gọi thực thi trên hàm này thông qua ICompanyRepository
        public ICompanyRepository Company => _companyRepository.Value;

        public IEmployeeRepository Employee => _employeeRepository.Value;

        // Sau khi triển khai CRUD thì có thể lưu lại ngay lập tức
        public void Save() => _repositoryContext.SaveChanges();
    }
}
