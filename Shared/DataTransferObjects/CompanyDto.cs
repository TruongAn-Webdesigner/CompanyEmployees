using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Shared.DataTransferObjects
{
    //
    /// <summary>
    /// DTO (dât transfer object) là 1 đối tượng dùng để di chuyển data giữa client và ứng dụng server, 
    /// nói đơn giản nó chính là cổng trung gian giữa models class(mấy cái class cấu hình kết nối bảng database) và api reponse, 
    /// giống như cái service.contracts,
    /// mục đích là nó sẽ giúp việc thay đổi models như thêm xóa cột cũng không ảnh hưởng đến data response mà client yêu cầu
    /// Giao tiếp: client -> service.contracts -> shared -> models (Entities)
    /// Lưu ý để truyền được dữ liệu cần để là record
    /// các tham số bên trong chính là tên array sẽ hiển thị ngoài postman
    /// </summary>


    //public record CompanyDto(Guid Id, string Name, string FullAddress);
    // thuộc tính này hỗ trợ chuyển json -> xml

    //[Serializable]
    [DataContract] // đổi sang dạng này để tránh (name_BackingField) trong xml + [DataMember]
    public record CompanyDto
    {
        // caấu hình tùy chọn cho dữ liệu trả về api xml
        // init giúp bao vệ biến không bị thay đổi khi qúa trình khỏi tạo kết thúc
        [DataMember]
        public Guid Id { get; init; }
        [DataMember]
        public string? Name { get; init; }
        [DataMember]
        public string? FullAddress { get; init; }
    }
}
