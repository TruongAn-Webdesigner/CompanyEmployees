using Application.Commands;
using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using MediatR;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Application.Handlers
{
    internal sealed class UpdateCompanyHandler : IRequestHandler<UpdateCompanyCommand, Unit>
    {
        private readonly IRepositoryManager _repository;
        private readonly IMapper _mapper;
        public UpdateCompanyHandler(IRepositoryManager repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
        {
            var CompanyEntity = _repository.Company.GetCompany(request.Id, request.TrackChanges);

            if (CompanyEntity is null)
            {
                throw new CompanyNotFoundException(request.Id);
            }

            _mapper.Map(request.CompanyForUpdateDto, CompanyEntity);

            await _repository.SaveAsync();

            return Unit.Value;
        }
    }
}
