using MediatR;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries
{
    public sealed record GetCompanyQuery(Guid Id, bool TrackChanges) :
        IRequest<CompanyDto>;
}
