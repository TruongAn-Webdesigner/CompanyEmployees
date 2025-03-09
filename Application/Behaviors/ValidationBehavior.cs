using Entities.Exceptions;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Behaviors
{
    //việc triển khai IValidation sẽ giúp fulent quét project tìm triển khai AbstractValidator cho 1 loại nhất định
    // rồi cung cáp 1 phiển bản khi chạy, vd CreateCompanyCommandValidator
    public sealed class ValidationBehavior<TRequest, TResponse> :
        IPipelineBehavior<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // nếu không có lỗi gọi next tìm validate tiếp theo
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);
            // nếu có lỗi thì extract ra từ validators sau đó nhóm nó vào dictionary,
            // nếu errorDictionary có tồn tại phần tử thì ném cái lỗi ra
            var errorDictionary = _validators
                .Select(x => x.Validate(context))
                .SelectMany(x => x.Errors)
                .Where(x => x != null)
                .GroupBy(
                    x => x.PropertyName.Substring(x.PropertyName.IndexOf('.') + 1),
                    x => x.ErrorMessage,
                    (propertyName, errorMessage) => new
                    {
                        key = propertyName,
                        Values = errorMessage.Distinct().ToArray()
                    })
                .ToDictionary(x => x.key, x => x.Values);

            if (errorDictionary.Any())
                throw new ValidationAppException(errorDictionary);

            return await next();
        }
    }
}
