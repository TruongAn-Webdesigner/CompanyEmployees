using CompanyEmployees.Presentation.Controllers.Extensions;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Application.Queries;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Marvin.Cache.Headers;
using CompanyEmployees.Presentation.ActionFilters;
using Application.Commands;
using Application.Notifications;

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
        private readonly ISender _sender; //33.3 triển khai mediatr
        private readonly IPublisher _publisher;

        public CompaniesV2Controller(IServiceManager serviceManager, ISender sender, IPublisher publisher)
        {
            _serviceManager = serviceManager;
            _sender = sender;
            _publisher = publisher;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            //var companies = await _serviceManager.CompanyService.GetAllCompanies(trackChanges: false);

            var companiesV2 = await _sender.Send(new GetCompaniesQuery(TrackChanges: false));

            //var companiesV2 = companies.Select(x => $"{x.Name} V2");
            return Ok(companiesV2);
        }

        [HttpGet("{id:guid}", Name = "CompanyById")]
        public async Task<IActionResult> GetCompany(Guid id)
        {
            var companyV2 = await _sender.Send(new GetCompanyQuery(id, TrackChanges: false));

            return Ok(companyV2);
        }

        [HttpPost(Name = "CreateCompany")]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto companyForCreationDto)
        {
            if (companyForCreationDto is null)
                return BadRequest("CompanyForCreationDto object is null");

            var createdCompany = await _sender.Send(new CreateCompanyCommand(companyForCreationDto));

            return CreatedAtRoute("CompanyById", new { id = createdCompany.Id }, createdCompany);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateCompany(Guid id, CompanyForUpdateDto companyForUpdateDto)
        {
            if (companyForUpdateDto is null)
                return BadRequest("CompanyForUpdateDto object is null");

            await _sender.Send(new UpdateCompanyCommand(id, companyForUpdateDto, TrackChanges: true));

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            //await _sender.Send(new DeleteCompanyCommand(id, TrackChanges: false));
            await _publisher.Publish(new CompanyDeletedNotification(id, TrackChanges: false));

            return NoContent();
        }
    }
}
