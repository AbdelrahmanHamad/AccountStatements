using FluentValidation;
using MediatR;
using AccountStatements.Domain.Entities;
using AccountStatements.Domain.Repositories;

namespace AccountStatements.Application.Features.Transactions.Commands.CreateTransaction
{
    public record CreateTransactionCommand(Guid CustomerId, decimal Amount, string Description, DateTime? TransactionDate) : IRequest<Guid>;

    public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
    {
        public CreateTransactionCommandValidator()
        {
            RuleFor(v => v.CustomerId)
                .NotEmpty().WithMessage("CustomerId is required.");

            RuleFor(v => v.Amount)
                .NotEqual(0).WithMessage("Amount cannot be zero.");

            RuleFor(v => v.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(250).WithMessage("Description must not exceed 250 characters.");
        }
    }

    public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, Guid>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateTransactionCommandHandler(
            ITransactionRepository transactionRepository,
            ICustomerRepository customerRepository,
            IUnitOfWork unitOfWork)
        {
            _transactionRepository = transactionRepository;
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer == null)
            {
                throw new ValidationException(new[] 
                {
                    new FluentValidation.Results.ValidationFailure("CustomerId", "Customer does not exist.")
                });
            }

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Description = request.Description,
                TransactionDate = request.TransactionDate ?? DateTime.UtcNow
            };

            await _transactionRepository.AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return transaction.Id;
        }
    }
}
