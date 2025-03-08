using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RequestFeatures
{
    // 16.4 phần cải tiến paging
    public class MetaData
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public bool HasPrevious => CurrentPage > 1; // trở về trước nếu page hiện tại lớn hơn 1

        // trở về sau nếu page hiện tại nhỏ hơn tổng page
        // tính bằng cách chia số mục với  kích thước trang và làm tròn nó, vì 1 page cũng tồn tại ngay cả khi có 1 mục
        // xem tại PageList class
        public bool HasNext => CurrentPage < TotalPages;
    }
}
