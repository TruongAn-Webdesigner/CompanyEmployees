﻿using Application.Queries;
using AutoMapper;
using Contracts;
using MediatR;
using Shared.DataTransferObjects;
using Entities.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers
{
    internal sealed class GetCompanyHandler : IRequestHandler<GetCompanyQuery, CompanyDto>
    {
        private readonly IRepositoryManager _repository;
        private readonly IMapper _mapper;
        public GetCompanyHandler(IRepositoryManager repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<CompanyDto> Handle(GetCompanyQuery request, CancellationToken
cancellationToken)
        {
            var company = await _repository.Company.GetCompany(request.Id, request.TrackChanges);

            if (company is null)
            {
                throw new CompanyNotFoundException(request.Id);
            }

            var companyDto = _mapper.Map<CompanyDto>(company);

            return companyDto;
        }
    }
}
