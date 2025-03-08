using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.LinkModels
{
    // class này đảm nhiệm chứa các links từ class Link
    public class LinkResourceBase
    {
        public LinkResourceBase() { }

        public List<Link> Links { get; set; } = new List<Link>();
    }
}
