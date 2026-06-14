using FluentValidation;
using MediatR;
using AccountStatements.Application.DTOs;
using AccountStatements.Application.Interfaces;
using AccountStatements.Domain.Entities;
using AccountStatements.Domain.Repositories;

namespace AccountStatements.Application.Features.Statements.Commands.GenerateCustomerStatement
{
    public record GenerateCustomerStatementCommand(Guid CustomerId, string Month) : IRequest<GenerateCustomerStatementResponse>;

    public record GenerateCustomerStatementResponse(string Message, Guid? StatementId, bool Success);

    public class GenerateCustomerStatementCommandValidator : AbstractValidator<GenerateCustomerStatementCommand>
    {
        public GenerateCustomerStatementCommandValidator()
        {
            RuleFor(v => v.CustomerId)
                .NotEmpty().WithMessage("CustomerId is required.");

            RuleFor(v => v.Month)
                .NotEmpty().WithMessage("Month is required.")
                .Matches(@"^\d{4}-(0[1-9]|1[0-2])$")
                .WithMessage("Month must be in YYYY-MM format (e.g., 2026-06).");
        }
    }

    public class GenerateCustomerStatementCommandHandler : IRequestHandler<GenerateCustomerStatementCommand, GenerateCustomerStatementResponse>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountStatementRepository _accountStatementRepository;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IUnitOfWork _unitOfWork;

        public GenerateCustomerStatementCommandHandler(
            ICustomerRepository customerRepository,
            ITransactionRepository transactionRepository,
            IAccountStatementRepository accountStatementRepository,
            IBackgroundJobService backgroundJobService,
            IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _transactionRepository = transactionRepository;
            _accountStatementRepository = accountStatementRepository;
            _backgroundJobService = backgroundJobService;
            _unitOfWork = unitOfWork;
        }

        public async Task<GenerateCustomerStatementResponse> Handle(GenerateCustomerStatementCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer == null)
            {
                return new GenerateCustomerStatementResponse($"Customer with ID '{request.CustomerId}' was not found.", null, false);
            }

            var exists = await _accountStatementRepository.ExistsAsync(customer.Id, request.Month, cancellationToken);
            if (exists)
            {
                return new GenerateCustomerStatementResponse($"Statement already exists for customer '{customer.Name}' for month {request.Month}.", null, false);
            }

            var parts = request.Month.Split('-');
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);

            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1).AddTicks(-1);

            var startingBalance = await _transactionRepository.GetBalanceBeforeDateAsync(customer.Id, startDate, cancellationToken);
            var monthlyTransactions = await _transactionRepository.GetTransactionsByCustomerAndPeriodAsync(customer.Id, startDate, endDate, cancellationToken);
            var endingBalance = startingBalance + monthlyTransactions.Sum(t => t.Amount);

            var statement = new AccountStatement
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                StatementMonth = request.Month,
                StartingBalance = startingBalance,
                EndingBalance = endingBalance,
                GeneratedAt = DateTime.UtcNow,
                EmailSentStatus = "Pending"
            };

            await _accountStatementRepository.AddAsync(statement, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var statementDto = new AccountStatementDto
            {
                Id = statement.Id,
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                CustomerEmail = customer.Email,
                StatementMonth = statement.StatementMonth,
                StartingBalance = statement.StartingBalance,
                EndingBalance = statement.EndingBalance,
                GeneratedAt = statement.GeneratedAt,
                EmailSentStatus = statement.EmailSentStatus,
                Transactions = monthlyTransactions.Select(t => new TransactionDto
                {
                    Id = t.Id,
                    CustomerId = t.CustomerId,
                    Amount = t.Amount,
                    Description = t.Description,
                    TransactionDate = t.TransactionDate
                }).ToList()
            };

            _backgroundJobService.Enqueue<IEmailJob>(job => job.SendEmailAndUpdateStatusAsync(statement.Id, statementDto, CancellationToken.None));

            return new GenerateCustomerStatementResponse($"Successfully enqueued statement generation and emailing for {customer.Name}.", statement.Id, true);
        }
    }
}
