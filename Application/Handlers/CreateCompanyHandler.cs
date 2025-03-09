using Application.Commands;
using AutoMapper;
using Contracts;
using Entities.Models;
using MediatR;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers
{
    internal sealed class CreateCompanyHandler : IRequestHandler<CreateCompanyCommand, CompanyDto>
    {
        private readonly IRepositoryManager _repository;
        private readonly IMapper _mapper;
        public CreateCompanyHandler(IRepositoryManager repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        
        public async Task<CompanyDto> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
        {
            var CompanyEntity = _mapper.Map<Company>(request.Company);

           _repository.Company.CreateCompany(CompanyEntity);

            await _repository.SaveAsync();

            var CompanyToReturn = _mapper.Map<CompanyDto>(CompanyEntity);

            return CompanyToReturn;
        }
    }
}
