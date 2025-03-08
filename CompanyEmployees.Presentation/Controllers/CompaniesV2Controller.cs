using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyEmployees.Presentation.Controllers
{
    //24.2 triển khai tạo versioning
    // với ApiVersion thì controller này triển khai là ver 2.0
    //[ApiVersion("2.0")]// có thể bỏ đi nếu khai báo config version chung 24.2.5
    [Route("api/companies")] // {v:apiversion} = ApiVersion setting hiện tại
    [ApiController]
    //30.2 triển khai swagger
    [ApiExplorerSettings(GroupName = "v2")]
    public class CompaniesV2Controller : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public CompaniesV2Controller(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _serviceManager.CompanyService.GetAllCompanies(trackChanges: false);

            var companiesV2 = companies.Select(x => $"{x.Name} V2");

            return Ok(companiesV2);
        }
    }
}
