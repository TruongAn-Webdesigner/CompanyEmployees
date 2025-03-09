using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Notifications
{
    // notification nó sẽ thông báo 1 sự kiện xảy ra trong hệ thống như thêm, xóa, sửa và nhiều handler có thể phản ứng từng loại 1 cách độc lập
    // trong trường hợp này sẽ là 1 thông báo hiển thị log cái gì đó đã bị xóa
    public sealed record CompanyDeletedNotification(Guid Id, bool TrackChanges) : INotification;
}
