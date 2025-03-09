using MediatR;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands
{
    // hàm này không trả về giá trị gì nên nó không kế thừa bất cứ T nào của IRequest
    public sealed record UpdateCompanyCommand(Guid Id, CompanyForUpdateDto CompanyForUpdateDto, bool TrackChanges) : IRequest<Unit>;
}
