using CompanyEmployees.Presentation.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyEmployees.Presentation.Controllers
{
    [Route("api/companies")] // cái này gọi là Attribute routing định tuyến thuộc tính
    // ApiController thuộc tính này có thể giúp giải quyết 1 phàn validate tự động như:
    // yêu cầu thuộc tính route (HttpGet,...)
    // phản hồi lại 400 (nọi dung phản hồi có trong models),
    // Suy luận tham số nguồn liên kết, ([FromBody])
    // Suy luận request multipart/form-data, khi upload, giúp truy cập dễ hơn vào dữ liệu đã upload
    // Tự động trả về chi tiết lỗi
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly IServiceManager _service;

        public CompaniesController(IServiceManager service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetCompanies()
        {
            //throw new Exception("Exception");
            //try => lọa bỏ try catch vì bây giờ đã có ExceptionMiddlewareExtensions xử lý exception rồi
            //{
                var companies = _service.CompanyService.GetAllCompanies(trackChanges: false);

                return Ok(companies);
            //}
            //catch
            //{
            //    return StatusCode(500, "Internal server error");
            //}
        }

        // do route đang là api/companies nên nó sẽ nối id vào
        // :guid là để xác định id này thuộc loại guid
        // Name là tên action có ích trong việc xử lý phương thực tạo mới company
        [HttpGet("{id:guid}", Name = "CompanyById")]
        public IActionResult GetCompany(Guid id)
        {
            var company = _service.CompanyService.GetCompany(id, trackChanges: false);
            return Ok(company);
        }

        // đây là phương thức tạo mới
        // phương thức này không thể lấy giá trị từ uri để tạo mới dữ liệu ví dụ: https:.../api/company/create?id=...
        // dữ liệu lấy từ giá trị nhập vào từ phần nội dung (mục Body -> raw) postman, nên cần phải có [FromBody] để nó nhận chính xác dự liệu từ Body
        // có thể xử lý theo uri = [FromUri],nhưng vì bảo mật nên không nên dùng cái này
        [HttpPost]
        public IActionResult CreateCompany([FromBody] CompanyForCreationDto company)
        {
            //khi dữ liệu nhập vào gửi qua request thì nó được đóng gói trong requestbody và server cố gắng chuyển đổi dữ liệu đó sang đối tượng (deserialization)
            // có thể khi chuyển đổi deserialization sẽ thất bại và null nên cần phải kiểm tra
            if (company is null)
                return BadRequest("CompanyForCreationDto object is null");

            var createdCompany = _service.CompanyService.CreateCompany(company);

            // CompanyById dựa vào name route get
            // CreatedAtRoute trả về 201
            // Post này không an toàn và không bình thường vì cứ send là 1 cột tạo mới nếu không có validate hoặc validate không kỹ
            return CreatedAtRoute("CompanyById", new { id = createdCompany.Id }, createdCompany); 
        }

        [HttpGet("collection/({ids})", Name = "CompanyCollection")]
        // chuyển sang dùng ModelBinder để nó tự động gán IE<string>(chuỗi guid) sang IE<guid>
        // hàm ModelBinder trongsẽ xử lý trước khi chạy vào 
        public IActionResult GetCompanyCollection(/*IEnumerable<Guid> ids*/ [ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
        {
            var companies = _service.CompanyService.GetByIds(ids, trackChanges: false);

            return Ok(companies);
        }

        // tạo nhều company trong 1 request
        [HttpPost("collection")]
        public IActionResult CreateCompanyCollection([FromBody] IEnumerable<CompanyForCreationDto> companyCollection)
        {
            var result = _service.CompanyService.CreateCompanyCollection(companyCollection);

            return CreatedAtRoute("CompanyCollection", new { result.ids }, result.companies);
        }

        [HttpDelete("{id:guid}")]
        public IActionResult DeleteCompany(Guid id)
        {
            _service.CompanyService.DeleteCompany(id, trackChanges: false);

            return NoContent();
        }

        // phần này vừa có thể update company + add mới nhân viên do có khai báo IE<em dto> trong CompanyForUpdateDto
        [HttpPut("{id:guid}")]
        public IActionResult UpdateCompany(Guid id,[FromBody] CompanyForUpdateDto company)
        {
            if (company is null)
                return BadRequest("CompanyForUpdateDto object is null");

            _service.CompanyService.UpdateCompany(id, company, trackChanges: true);
            return NoContent();
        }
    }
}
