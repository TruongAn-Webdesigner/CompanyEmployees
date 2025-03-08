using Shared.RequestFeatures;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.LinkModels
{
    //21.5 record này giúp chuyển tham số từ controller đến service layer
    public record LinkParameters(EmployeeParameters EmployeeParameters, HttpContext HttpContent);
}
