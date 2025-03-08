using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RequestFeatures
{
    // lớp này giúp tạo các thuộc tính chung cho tất cả các class kế thừa
    public abstract class RequestParameters
    {
        const int maxPageSize = 50;
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }

        // 19.2 mở rộng tính năng sort
        public string? OrderBy { get; set; }

        //20.2 mở rộng tính năng data shaping - chọn filed hiển thị
        public string? Fields { get; set; }
    }
}
