using MediatR;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands
{
    // đây là nơi truyền cách command dạng save (hoặc delete nếu có tạo 1 class giống thế này)
    public sealed record CreateCompanyCommand(CompanyForCreationDto Company) :
        IRequest<CompanyDto>;
}
