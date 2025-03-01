using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CompanyEmployees.Presentation.ModelBinders
{
    public class ArrayModelBinder : IModelBinder
    {
        // tạo 1 binder cho loại generic(T) của IEnumerable
        // nghĩa là class này có thể xử lý linh hoạt nhiều kiểu khác nhau của IEnumerable<int>, string,...
        // sau đó convert trả về kiểu IE<phù hợp> sau khi phân tích
        // ví dụ kiểu guid khi client nhập gửi qua request nó là dạng IE<string> nên cần chuyển nó sang IE<guid> bằng cách đoán converter
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // kiểm tra loại tham số IEnumerable đó có đúng là IEnum không
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            // trích xuất chuỗi giá trị (chuỗi GUIDs phân tách bằng dấu phẩy) bằng ValueProvider.GetValue
            // vì là chuỗi nên chỉ cần check null hoặc empty, trả về null nếu đúng và bên controller có check null rồi
            var providedValue = bindingContext.ValueProvider
                .GetValue(bindingContext.ModelName)
                .ToString();
            if (string.IsNullOrEmpty(providedValue))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }
            // lưu trữ loại IEnum
            var genericType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];
            // chuyển sang IEnum generic phù hợp, trong trường hợp này là GUID chứ không phải là chuyển sang GUID
            var converter = TypeDescriptor.GetConverter(genericType);

            // mảng object này có thể chứa các phần tử thuộc bất kỳ kiểu dữ liệu nào (value types, reference types, boxed values,...)
            // vì ban đầu model bind nhận từ request nó là chuỗi string, và hiện tại providedValue đã được convert sang string
            // đây chính là nơi chứa các GUID được nhập vào từ client
            var objectArray = providedValue.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => converter.ConvertFromString(x.Trim()))
                .ToArray();

            // khởi tạo mảng mới cùng chiều dài từ ojectArray
            var guidArray = Array.CreateInstance(genericType, objectArray.Length);
            objectArray.CopyTo(guidArray, 0);// lấy các mảng từ objectArray
            bindingContext.Model = guidArray; // gán mảng được chuyển đổi sang GUID vào bindingContext

            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;
        }
    }
}
