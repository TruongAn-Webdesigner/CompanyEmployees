using Entities.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IDataShaper<T>
    {
        // Shape cho 1 danh sách các entity
        IEnumerable<ShapedEntity> ShapeData(IEnumerable<T> entities, string fieldsString);

        // Shape cho 1 entity đơn lẻ
        //20.4 ExpandoObject -> Entity
        //21.3 Entity -> ShapedEntity
        ShapedEntity ShapeData(T entity, string fieldsString);
    }
}
