using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    // 21.3 triẻn khai entiry và id cho HATEOAS vì nó cần id để hoạt động
    public class ShapedEntity
    {
        public ShapedEntity()
        {
            Entity = new Entity();
        }

        public Guid Id { get; set; }
        public Entity Entity { get; set; }
    }
}
