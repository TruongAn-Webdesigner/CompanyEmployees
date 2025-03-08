using CompanyEmployees.Presentation.ActionFilters;
using CompanyEmployees.Presentation.ModelBinders;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyEmployees.Presentation.Controllers
{
    // 24.2 triển khai api version, mặc định 1.0 nếu không set do có câú hình opt.AssumeDefaultVersionWhenUnspecified = true;
    //[ApiVersion("1.0")]// có thể bỏ đi nếu khai báo config version chung 24.2.5
    [Route("api/companies")] // cái này gọi là Attribute routing định tuyến thuộc tính
    // ApiController thuộc tính này có thể giúp giải quyết 1 phàn validate tự động như:
    // yêu cầu thuộc tính route (HttpGet,...)
    // phản hồi lại 400 (nọi dung phản hồi có trong models),
    // Suy luận tham số nguồn liên kết, ([FromBody])
    // Suy luận request multipart/form-data, khi upload, giúp truy cập dễ hơn vào dữ liệu đã upload
    // Tự động trả về chi tiết lỗi
    [ApiController]
    //30.2 triển khai swagger
    [ApiExplorerSettings(GroupName = "v1")]

    //25.3 triển khai cache-store loại cache profiles
    //[ResponseCache(CacheProfileName = "120SecondsDuration")]
    public class CompaniesController : ControllerBase
    {
        private readonly IServiceManager _service;

        public CompaniesController(IServiceManager service)
        {
            _service = service;
        }

        // 15.2 cái này là khai báo action filter cho chính hàm này giúp làm cái gì đó trước rồi tới hàm GetCompanies rồi đến actionfilter làm cái gì đó cuối cùng
        /// <summary>
        /// Gets the list of all companies
        /// </summary>
        /// <returns>The companies list</returns>
        [ServiceFilter(typeof(AsyncActionFilterExample))] 
        [HttpGet(Name = "GetCompanies")]
        // 27.7 triển khai protect end point
        //[Authorize] // chỉ định hành động hoặc controller áp dụng yêu cầu authorization
        [Authorize(Roles = "Manager")] //27.9 chỉ role Manager mới đươc truy cập
        public async Task<IActionResult> GetCompanies()
        {
            //throw new Exception("Exception");
            //try => lọa bỏ try catch vì bây giờ đã có ExceptionMiddlewareExtensions xử lý exception rồi
            //{
                var companies = await _service.CompanyService.GetAllCompanies(trackChanges: false);

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
        //25.2 triển khai cache
        [ResponseCache(Duration = 60)]
        //25.6.1 tùy chọn cấu hình cache
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = 60)] // cache này ưu tiên trước global
        [HttpCacheValidation(MustRevalidate = false)]
        public async Task<IActionResult> GetCompany(Guid id)
        {
            var company = await _service.CompanyService.GetCompany(id, trackChanges: false);
            return Ok(company);
        }


        // luôn đặt returns ở cuối không thì nó không hiển thị
        /// <summary>
        /// Creates a newly created company
        /// </summary>
        /// <param name="company"></param>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is null</response>
        /// <response code="422">If the model is invalid</response>
        /// <returns>A newly created company</returns>

        // đây là phương thức tạo mới
        // phương thức này không thể lấy giá trị từ uri để tạo mới dữ liệu ví dụ: https:.../api/company/create?id=...
        // dữ liệu lấy từ giá trị nhập vào từ phần nội dung (mục Body -> raw) postman, nên cần phải có [FromBody] để nó nhận chính xác dự liệu từ Body
        // có thể xử lý theo uri = [FromUri],nhưng vì bảo mật nên không nên dùng cái này
        // phần này vừa có thể tạo company + add mới nhân viên do có khai báo IE<em dto> trong CompanyForCreationDto
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [HttpPost(Name = "CreateCompany")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto company)
        {
            //khi dữ liệu nhập vào gửi qua request thì nó được đóng gói trong requestbody và server cố gắng chuyển đổi dữ liệu đó sang đối tượng (deserialization)
            // có thể khi chuyển đổi deserialization sẽ thất bại và null nên cần phải kiểm tra
            // do dùng action filter nên không cần validate chỗ này
            //if (company is null)
            //    return BadRequest("CompanyForCreationDto object is null");

            //if (!ModelState.IsValid)
            //    return UnprocessableEntity(ModelState);

            var createdCompany = await _service.CompanyService.CreateCompany(company);

            // CompanyById dựa vào name route get
            // CreatedAtRoute trả về 201
            // Post này không an toàn và không bình thường vì cứ send là 1 cột tạo mới nếu không có validate hoặc validate không kỹ
            return CreatedAtRoute("CompanyById", new { id = createdCompany.Id }, createdCompany); 
        }

        // chuyển sang dùng ModelBinder để nó tự động gán IE<string>(chuỗi guid) sang IE<guid>
        // hàm ModelBinder sẽ xử lý trước khi chạy vào trong
        [HttpGet("collection/({ids})", Name = "CompanyCollection")]
        public async Task<IActionResult> GetCompanyCollection(/*IEnumerable<Guid> ids*/ [ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
        {
            var companies = await _service.CompanyService.GetByIds(ids, trackChanges: false);

            return Ok(companies);
        }

        // tạo nhều company trong 1 request
        [HttpPost("collection")]
        public async Task<IActionResult> CreateCompanyCollection([FromBody] IEnumerable<CompanyForCreationDto> companyCollection)
        {
            var result = await _service.CompanyService.CreateCompanyCollection(companyCollection);

            return CreatedAtRoute("CompanyCollection", new { result.ids }, result.companies);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            await _service.CompanyService.DeleteCompany(id, trackChanges: false);

            return NoContent();
        }

        // phần này vừa có thể update company + add mới nhân viên do có khai báo IE<em dto> trong CompanyForUpdateDto
        [HttpPut("{id:guid}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateCompany(Guid id,[FromBody] CompanyForUpdateDto company)
        {
            // do dùng action filter nên không cần validate chỗ này
            //if (company is null)
            //    return BadRequest("CompanyForUpdateDto object is null");

            await _service.CompanyService.UpdateCompany(id, company, trackChanges: true);
            return NoContent();
        }

        //22.2 triển khai request loại options
        [HttpOptions]
        public IActionResult GetCompaniesOptions()
        {
            // khi goi options nó sẽ trả về allow... trong headers
            Response.Headers.Add("Allow", "GET, OPTIONS, POST");

            return Ok();
        }

        
    }
}
