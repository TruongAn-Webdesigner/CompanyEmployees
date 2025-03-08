using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RequestFeatures
{
    public class PagedList<T> : List<T>
    {
        public MetaData MetaData { get; set; }

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            // Metadata class đã tạo hữu ích cho response 
            MetaData = new MetaData
            {
                TotalCount = Count,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize)
            };

            AddRange(items);
        }

        public static PagedList<T> ToPagedList(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = source
            // bỏ qua mấy kết quả sau và lấy kết quả trước đo (3-1) * 20 = 40, bỏ qua 40 kết quả
            .Skip((pageNumber - 1) * pageSize)
            // lấy kết quả còn lại
            .Take(pageSize)
            .ToList();

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
