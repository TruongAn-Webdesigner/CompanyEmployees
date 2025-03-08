using Contracts;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class DataShaper<T> : IDataShaper<T> where T : class
    {
        // mangr property sẽ chứa bất kể là loại gì - Company, Employee
        public PropertyInfo[] Properties { get; set; }

        public DataShaper()
        {
            // lấy property tại hàm khởi tạo
            Properties = typeof(T).GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance);
        }

        public IEnumerable<ShapedEntity> ShapeData(IEnumerable<T> entities, string fieldsString)
        {
            var requiredProperties = GetRequiredProperties(fieldsString);

            return FetchData(entities, requiredProperties);
        }
        // cả 2 ShapeData đều làm nhiệm vụ nạp các filed từ GetRequiredProperties
        public ShapedEntity ShapeData(T entity, string fieldsString)
        {
            var requiredProperties = GetRequiredProperties(fieldsString);

            return FetchDataForEntity(entity, requiredProperties);
        }

        // hàm này nó sẽ phần tích chuỗi đầu vào và trả về đúng cái filed mình muốn cho controller
        private IEnumerable<PropertyInfo> GetRequiredProperties(string fieldsString)
        {
            var requiredProperties = new List<PropertyInfo>();

            if (!string.IsNullOrWhiteSpace(fieldsString))
            {
                var fields = fieldsString.Split(',', StringSplitOptions.RemoveEmptyEntries);

                foreach (var field in fields)
                {
                    // kiểm tra property có giống với properties trong entity không
                    var property = Properties
                        .FirstOrDefault(pi => pi.Name.Equals(field.Trim(), StringComparison.InvariantCultureIgnoreCase));

                    if (property == null)
                        continue;
                    // có thì add
                    requiredProperties.Add(property);
                }
            }
            else
            {
                // nếu trống thì trả về tất cả properties
                requiredProperties = Properties.ToList();
            }

            return requiredProperties;
        }

        // trích xuất các giá trị từ các thuộc tính được GetRequiredProperties trả về filed
        // xử lý cho nhiều entity
        private IEnumerable<ShapedEntity> FetchData(IEnumerable<T> entities, IEnumerable<PropertyInfo> requiredProperties)
        {
            var shapeData = new List<ShapedEntity>();

            foreach (var entity in entities)
            {
                var shapedObject = FetchDataForEntity(entity, requiredProperties);
                shapeData.Add(shapedObject);
            }

            return shapeData;
        }

        // trích xuất các giá trị từ các thuộc tính được GetRequiredProperties trả về filed
        // xử lý cho entity đơn
        // 20.4 đổi ExpandoObject sang Entity
        // 21.3 đổi Entity sang ShapedEntity
        private ShapedEntity FetchDataForEntity(T entity, IEnumerable<PropertyInfo> requiredProperties)
        {
            var shapedObject = new ShapedEntity();

            foreach (var property in requiredProperties)
            {
                // trích xuất giá trị thêm vào ExpandoObject
                var objectPropertyValue = property.GetValue(entity);
                //ExpandoObject này là IDictionary<string, object?> nên dùng TryAdd để thêm, Name = keys - objectPropertyValue = values
                shapedObject.Entity.TryAdd(property.Name, objectPropertyValue);
            }
            var objectProperty = entity.GetType().GetProperty("Id");
            shapedObject.Id = (Guid)objectProperty.GetValue(entity);
            return shapedObject;
        }
    }
}
