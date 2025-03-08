using Entities.Models;
using Repository.Extensions.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Repository.Extensions
{
    public static class RepositoryEmployeeExtensions
    {
        public static IQueryable<Employee> FilterEmployees(this IQueryable<Employee> employees, uint minAge, uint maxAge)
            => employees.Where(e => (e.Age >= minAge && e.Age <= maxAge));

        public static IQueryable<Employee> Search(this IQueryable<Employee> employees, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return employees;

            var lowerCaseTerm = searchTerm.Trim().ToLower();

            return employees.Where(e => e.Name.ToLower().Contains(lowerCaseTerm));
        }

        public static IQueryable<Employee> Sort(this IQueryable<Employee> employees, string orderByQueryString)
        {
            // nếu gửi https://localhost:5001/api/companies/companyId/employees?orderBy = name,age desc
            // orderByQueryString = name,age desc

            // kiểm tra orderByQueryString rỗng, nếu có trả về danh sách như cũ
            if (string.IsNullOrWhiteSpace(orderByQueryString))
                return employees.OrderBy(e => e.Name);

            //19.5 nâng cao sort dùng chung với T
            var orderQuery = OrderQueryBuilder.CreateOrderQuery<Employee>(orderByQueryString);

            // tách name, age, desc thành array
            var orderParams = orderByQueryString.Trim().Split(',');

            // chuẩn bị danh sách chứa các đối tượng loại PropertyInfor đại diện các thuộc tính lớp Employee
            // và kiểm tra xem trường nhận có tồn tại đúng trong Employee hay không
            var propertyInfos = typeof(Employee).GetProperties(System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance);
            var orderQueryBuider = new StringBuilder();

            foreach (var param in orderParams)
            {
                // nếu rỗng thì bỏ qua
                if (string.IsNullOrWhiteSpace(param))
                    continue;

                // lấy propertyInfos kiểm tra và trả về đối tượng tìm thấy với propertyFromQueryName
                var propertyFromQueryName = param.Split(" ")[0];
                var objectProperty = propertyInfos.FirstOrDefault(pi => pi.Name.Equals(propertyFromQueryName,
                    StringComparison.InvariantCultureIgnoreCase));
                // nếu không thì bỏ qua
                if (objectProperty == null)
                    continue;
                // nếu có thì kiểm tra có tham số desc cuối chuỗi không
                var direction = param.EndsWith(" desc") ? "descending" : "ascending";

                // dùng string builder để xây dựng truy vấn trong mỗi vong lặp
                orderQueryBuider.Append($"{objectProperty.Name.ToString()} {direction},");
            }
            // xóa dấu , thừa trong chuỗi
            // var orderQuery = orderQueryBuider.ToString().TrimEnd(',', ' ');
            // nếu không có thì trả về danh sách như cũ
            if (string.IsNullOrWhiteSpace(orderQuery))
                return employees.OrderBy(e => e.Name);
            // bắt đầu sắp xếp theo Name asc và dateofbirth decs
            return employees.OrderBy(orderQuery);
        }
    }
}
