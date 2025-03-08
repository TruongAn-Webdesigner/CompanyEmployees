using CompanyEmployees.Presentation.ActionFilters;
using Entities.Exceptions;
using Entities.LinkModels;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CompanyEmployees.Presentation.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly IServiceManager _service;

        public EmployeesController(IServiceManager service) => _service = service;

        // tham số companyId được lấy trực tiếp từ route
        // lấy tất cả nhân viên
        // 16 thêm paging EmployeeParameters lấy từ uri FromQuery
        // 16.4 thêm MetaData để xử dụng chung paging
        //22.3 triển khai request loại HEAD
        [HttpGet]
        [HttpHead]
        [ServiceFilter(typeof(ValidateMediaTypeAttribute))] // 21.4.2 triển khai kiểm tra loại custom media
        public async Task<IActionResult> GetEmployeesForCompany(Guid companyId, [FromQuery] EmployeeParameters employeeParameters)
        {
            var linkParams = new LinkParameters(employeeParameters, HttpContext);

            var result = await _service.EmployeeService.GetEmployees(companyId, linkParams, trackChanges: false);

            //X-Pagination thêm vào header để hiển thị toàn bộ thông tin của metaData đang chứa
            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(result.metaData));

            return result.LinkResponse.HasLinks ? Ok(result.LinkResponse.LinkedEntities) :
                Ok(result.LinkResponse.ShapedEntities);
        }

        [HttpGet("{id:guid}", Name = "GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid id)
        {
            var employee = await _service.EmployeeService.GetEmployee(companyId, id, trackChanges: false);
            return Ok(employee);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employee)
        {
            //if (employee is null)
            //    return BadRequest("EmployeeForCreationDto object is null");

            //// 13.3 đây cũng là 1 kiểu validate custom dùng chung điều kiện và in ra 1 lỗi
            //if (employee.Name == "abc")
            //    ModelState.AddModelError("Name", "This name has been taken.");

            // 13.3 validate
            //if (!ModelState.IsValid)
            //    return UnprocessableEntity(ModelState);

            var employeeToReturn = await _service.EmployeeService.CreateEmployeeForCompany(companyId, employee, trackingChanges: false);

            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, employeeToReturn);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            await _service.EmployeeService.DeleteEmployeeForCompany(companyId, id, trackChanges: false);

            return NoContent();
        }

        // phần này xử lý cho put từ client
        [HttpPut("{id:guid}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] EmployeeForUpdateDto employee)
        {
            // do có action filter nên không cần dùng
            //if (employee is null)
            //    return BadRequest("EmployeeForUpdateDto object is null");

            //if (!ModelState.IsValid)
            //    return UnprocessableEntity(ModelState);

            await _service.EmployeeService.UpdateEmployeeForCompany(companyId, id, employee, compTrackChanges: false, empTrackChages: true);

            return NoContent();
        }

        // phần này xử lý cho patch json từ client
        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(
            Guid companyId, Guid id, [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
        {
            if (patchDoc is null)
                return BadRequest("patchDoc object sent from client is null.");

            // gọi đến ánh xạ từ loại Employee sang loại EmployeeForUpdateDto
            // vì patchdoc chỉ áp dụng mỗi EmployeeForUpdateDto
            var result = await _service.EmployeeService.GetEmployeeForPatch(companyId, id, compTrackChanges: false, empTrachChanges: true);

            //13.5 thêm validate ModelState cho applyto nhưng nó về cơ bản nếu giá trị là int khi save null là 0 mặc định
            // ban đầu vấn đề không phải do map chuyển loại hay kiểu dữ liệu nên gây ra lỗi vượt qua validate -> save data success
            // mà là do applyto chỉ thực thi nhiệm vụ là ghi json vào đối tượng và nó không có nhiệm vụ phải kiểm tra validate
            // trả về cho modelstate, ModelState trong applyto chỉ có nhiệm vụ thu thập lỗi từ applyto
            // nên ở trường khợp = 0 vẫn cho là hợp lệ vì applyto không được thiết kế để thực hiện validation dữ liệu
            // nó chỉ có thể kiểm tra cá lỗi cơ bản như lỗi path không hợp lệ, lỗi kiểu dữ liệu cơ bản, lỗi cú pháp JSON Patch
            patchDoc.ApplyTo(result.employeeToPatch, ModelState);

            TryValidateModel(result.employeeToPatch); // và cần kiểm tra trực tiếp tính hợp lệ bên trong giá trị

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            // gọi ánh xạ tiếp từ EmployeeForUpdateDto sang loại Employee
            _service.EmployeeService.SaveChangesForPatch(result.employeeToPatch, result.employeeEntity);

            return NoContent();
        }

    }
}
