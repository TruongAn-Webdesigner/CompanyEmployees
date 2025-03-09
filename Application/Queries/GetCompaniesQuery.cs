using MediatR;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries
{
    // triển khai phần query thông qua gửi entities đến DTOs
    public sealed record GetCompaniesQuery(bool TrackChanges) :
    IRequest<IEnumerable<CompanyDto>>;
}
