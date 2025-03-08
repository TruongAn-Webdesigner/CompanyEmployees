using Contracts;
using Entities.LinkModels;
using Entities.Models;
using Microsoft.Net.Http.Headers;
using Shared.DataTransferObjects;


namespace CompanyEmployees.Utility
{
    //21.5 triển khai HATEOAS
    public class EmployeeLinks : IEmployeeLinks
    {
        private readonly LinkGenerator _linkGenerator; // tạo liên kết phản hồi
        private readonly IDataShaper<EmployeeDto> _dataShaper; // IDataShaper tạo định hình dữ liệu

        public EmployeeLinks(LinkGenerator linkGenerator, IDataShaper<EmployeeDto> dataShaper)
        {
            _linkGenerator = linkGenerator;
            _dataShaper = dataShaper;
        }

        public LinkResponse TryGenerateLinks(IEnumerable<EmployeeDto> employeeDto, string fields, Guid companyId, HttpContext httpContext)
        {
            var shapedEmployees = ShapeData(employeeDto, fields);

            if (ShouldGenerateLinks(httpContext))
            {
                return ReturnLinkedEmployees(employeeDto, fields, companyId, httpContext, shapedEmployees);
                // cái quan trọng httpContext chứa media type
                // fields là loại field tùy chọn (shapeEntity)
            }

            return ReturnShapedEmployees(shapedEmployees);
        }

        // phương thức này chỉ trích phần thực thể không có id
        private List<Entity> ShapeData(IEnumerable<EmployeeDto> employeeDto, string fields)
            => _dataShaper.ShapeData(employeeDto, fields)
            .Select(e => e.Entity)
            .ToList();

        private bool ShouldGenerateLinks(HttpContext httpContext)
        {
            // tách media type kiểm tra nếu nó là hateoas thì true, ngươc lại false
            var mediaType = (MediaTypeHeaderValue)httpContext.Items["AcceptHeaderMediaType"];
            return mediaType.SubTypeWithoutSuffix.EndsWith("hateoas",
           StringComparison.InvariantCultureIgnoreCase);
        }

        // phương thức này chỉ trả về 1 link response mới với shapedEntities được gán và mặc đinh HasLink trong LinkResponse = false
        private LinkResponse ReturnShapedEmployees(List<Entity> shapedEmplyees) =>
            new LinkResponse { ShapedEntities = shapedEmplyees };

        private LinkResponse ReturnLinkedEmployees(IEnumerable<EmployeeDto> employeeDto, string fields, Guid companyId, HttpContext httpContext, List<Entity> shapedEmployees)
        {
            var employeeDtoList = employeeDto.ToList();

            // lặp qua từng nhân viên
            for (var index = 0; index < employeeDtoList.Count(); index++)
            {
                // CreateLinksForEmployee là method tạo link
                var employeeLinks = CreateLinksForEmployee(httpContext, companyId, employeeDtoList[index].Id, fields);
                // thêm vào danh sách shapedEmployees
                shapedEmployees[index].Add("Links", employeeLinks);
            }
            // gói lại thành 1 collection mới
            var employeeCollection = new LinkCollectionWrapper<Entity>(shapedEmployees);
            // tạo các link từ collection
            var linkedEmployees = CreateLinksForEmployees(httpContext, employeeCollection);

            return new LinkResponse { HasLinks = true, LinkedEntities = linkedEmployees };
        }

        private List<Link> CreateLinksForEmployee(HttpContext httpContext, Guid companyId, Guid id, string fields = "")
        {
            var links = new List<Link>
            {
                new Link(_linkGenerator.GetUriByAction(httpContext, "GetEmployeeForCompany", values: new
                {
                    companyId, id, fields
                }),
                "self",
                "GET"),
                new Link(_linkGenerator.GetUriByAction(httpContext, "DeleteEmployeeForCompany", values: new
                {
                    companyId, id
                }),
                "delete_employee",
                "Delete"),
                new Link(_linkGenerator.GetUriByAction(httpContext, "UpdateEmployeeForCompany", values: new
                {
                    companyId, id
                }),
                "update_emplyee",
                "PUT"),
                new Link(_linkGenerator.GetUriByAction(httpContext, "PartiallyUpdateEmployeeForCompany", values: new
                {
                    companyId, id
                }),
                "partially_update_employee",
                "PATCH")
            };

            return links;
        }

        private LinkCollectionWrapper<Entity> CreateLinksForEmployees(HttpContext httpContext, LinkCollectionWrapper<Entity> employeesWrapper)
        {
            //tạo links từ hàm của LinkGenerator cháp nhận loại httpcontext, action name, value cần dùng để làm url hợp lệ
            // trong trường hợp của EmployeesController, gửi company id, employee id, fields
            employeesWrapper.Links.Add(new Link(_linkGenerator.GetUriByAction(httpContext, "GetEmployeesForCompany", values: new { }),
                "self",
                "Get"));

            return employeesWrapper;
        }
    }
}
