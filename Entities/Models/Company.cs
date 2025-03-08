using Entities.LinkModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Entities.Models
{
    public class Company
    {
        [Column("Company")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Company name is a required field.")]
        [MaxLength(60, ErrorMessage = "Maximum length for the Name is 60 characters.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Company address is a required field.")]
        [MaxLength(60, ErrorMessage = "Maximum length for the Address is 60 characters")]
        public string? Address { get; set; }

        public string? Country { get; set; }

        public ICollection<Employee>? Employees { get; set; }

        //private void WriteLinksToXml(string key, object value, XmlWriter writer)
        //{
        //    writer.WriteStartElement(key);

        //    // nếu giá trị là loại link thì loop
        //    if (value.GetType() == typeof(List<Link>))
        //    {
        //        foreach (var val in value as List<Link>)
        //        {
        //            // gọi phương thức xử lý từng thuộc tính
        //            writer.WriteStartElement(nameof(Link));
        //            WriteLinksToXml(nameof(val.Href), val.Href, writer);
        //            WriteLinksToXml(nameof(val.Rel), val.Rel, writer);
        //            WriteLinksToXml(nameof(val.Method), val.Method, writer);
        //            writer.WriteEndElement();
        //        }
        //    }
        //    else
        //    {
        //        writer.WriteString(value.ToString());
        //    }

        //    writer.WriteEndElement();
        //}
    }
}
