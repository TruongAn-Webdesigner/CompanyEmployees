using Application.Notifications;
using Contracts;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers
{
    // giả xử email bị xóa và tạo thông báo vào log như mail đã gửi thông báo thành công
    internal sealed class EmailHandler : INotificationHandler<CompanyDeletedNotification>
    {
        private readonly ILoggerManager _logger;
        public EmailHandler(ILoggerManager logger) => _logger = logger;

        public async Task Handle(CompanyDeletedNotification notification, CancellationToken cancellationToken)
        {
            _logger.LogWarn($"Delete action for the company with id: { notification.Id} has occurred.");

            await Task.CompletedTask;
        }
    }
}
