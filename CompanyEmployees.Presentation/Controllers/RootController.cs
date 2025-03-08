using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.LinkModels;

namespace CompanyEmployees.Presentation.Controllers
{
    // không public mặc định kiểu internal -> kiểu này api không goi được
    [Route("api")]
    [ApiController]
    public class RootController : ControllerBase
    {
        //23.1 triển khai uri root
        // field này chứa liên kết hướng tới hành động api
        // hành động này chỉ thực thi request duy nhất /api
        // chưa biết gì, chưa triển khai được
        private readonly LinkGenerator _linkGenerator;

        public RootController (LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        // cái này chỉ hoạt động khi dùng phương thức Accept từ header 
        [HttpGet(Name = "GetRoot")]
        public IActionResult GetRoot([FromHeader(Name = "Accept")] string mediaType)
        {
            if (mediaType.Contains("application/vnd.codemaze.apiroot"))
            {
                // tạo liên kết HATEOAS
                var list = new List<Link>
                {
                    new Link
                    {
                        // dùng GetUriByName của LinkGenerator thì mới tạo được liên kết
                        Href = _linkGenerator.GetUriByName(HttpContext, nameof(GetRoot), new {}),
                        Rel = "self",
                        Method = "GET"
                    },
                    new Link
                    {
                        Href = _linkGenerator.GetUriByName(HttpContext, "GetCompanies", new {}),
                        Rel = "companies",
                        Method = "GET"
                    },
                    new Link
                    {
                        Href = _linkGenerator.GetUriByName(HttpContext, "CreateCompany", new {}),
                        Rel = "create_company",
                        Method = "POST"
                    }
                };

                return Ok(list);
            }

            return NoContent();
        }
    }
}
