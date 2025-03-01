using Entities.Exceptions;
using Microsoft.AspNetCore.JsonPatch;
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
    [Route("api/companies/{companyId}/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly IServiceManager _service;

        public EmployeesController(IServiceManager service) => _service = service;

        // tham số companyId được lấy trực tiếp từ route
        [HttpGet]
        public IActionResult GetEmplyeesForCompany(Guid companyId)
        {
            var employees = _service.EmployeeService.GetEmployees(companyId, trackChanges: false);
            return Ok(employees);
        }

        [HttpGet("{id:guid}", Name = "GetEmployeeForCompany")]
        public IActionResult GetEmployeeForCompany(Guid companyId, Guid id)
        {
            var employee = _service.EmployeeService.GetEmployee(companyId, id, trackChanges: false);
            return Ok(employee);
        }

        [HttpPost]
        public IActionResult CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employee)
        {
            if (employee is null)
                return BadRequest("EmployeeForCreationDto object is null");

            // 13.3 đây cũng là 1 kiểu validate custom dùng chung điều kiện và in ra 1 lỗi
            if (employee.Name == "abc")
                ModelState.AddModelError("Name", "This name has been taken.");

            // 13.3 validate
            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            var employeeToReturn = _service.EmployeeService.CreateEmployeeForCompany(companyId, employee, trackingChanges: false);

            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, employeeToReturn);
        }

        [HttpDelete("{id:guid}")]
        public IActionResult DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            _service.EmployeeService.DeleteEmployeeForCompany(companyId, id, trackChanges: false);

            return NoContent();
        }

        // phần này xử lý cho put từ client
        [HttpPut("{id:guid}")]
        public IActionResult UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] EmployeeForUpdateDto employee)
        {
            if (employee is null)
                return BadRequest("EmployeeForUpdateDto object is null");

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            _service.EmployeeService.UpdateEmployeeForCompany(companyId, id, employee, compTrackChanges: false, empTrackChages: true);

            return NoContent();
        }

        // phần này xử lý cho patch json từ client
        [HttpPatch("{id:guid}")]
        public IActionResult PartiallyUpdateEmployeeForCompany(
            Guid companyId, Guid id, [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
        {
            if (patchDoc is null)
                return BadRequest("patchDoc object sent from client is null.");

            // gọi đến ánh xạ từ loại Employee sang loại EmployeeForUpdateDto
            // vì patchdoc chỉ áp dụng mỗi EmployeeForUpdateDto
            var result = _service.EmployeeService.GetEmployeeForPatch(companyId, id, compTrackChanges: false, empTrachChanges: true);

            //13.5 thêm validate ModelState cho applyto nhưng nó về cơ bản nếu giá trị là int khi save null là 0 mặc định
            // thì employeeToPatch age = 0 không sai và !ModelState.IsValid vẫn cho là hợp lệ save change bình thường
            // nên mình cần thêm TryValidateModel(result.employeeToPatch); để kiểm tra thẳng giá trị bên trong nó
            patchDoc.ApplyTo(result.employeeToPatch, ModelState);

            TryValidateModel(result.employeeToPatch); // kiểm tra trực tiếp tính hợp lệ bên trong giá trị

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);

            // gọi ánh xạ tiếp từ EmployeeForUpdateDto sang loại Employee
            _service.EmployeeService.SaveChangesForPatch(result.employeeToPatch, result.employeeEntity);

            return NoContent();
        }
    }
}
