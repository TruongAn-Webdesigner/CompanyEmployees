using Application.Commands;
using Application.Notifications;
using Contracts;
using Entities.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers
{
    internal sealed class DeleteCompanyHandler : INotificationHandler<CompanyDeletedNotification> /*IRequestHandler<DeleteCompanyCommand, Unit>*/
    {
        private readonly IRepositoryManager _repository;
        public DeleteCompanyHandler(IRepositoryManager repository) => _repository = repository;

        public async Task Handle(/*DeleteCompanyCommand request*/ CompanyDeletedNotification notification, CancellationToken cancellationToken)
        {
            var company = await _repository.Company.GetCompany(notification.Id, notification.TrackChanges);

            if (company is null)
                throw new CompanyNotFoundException(notification.Id);

            _repository.Company.DeleteCompany(company);

            await _repository.SaveAsync();
        }
    }
}
