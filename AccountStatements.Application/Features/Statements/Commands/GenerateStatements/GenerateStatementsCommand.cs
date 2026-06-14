using FluentValidation;
using MediatR;
using AccountStatements.Application.DTOs;
using AccountStatements.Application.Interfaces;
using AccountStatements.Domain.Entities;
using AccountStatements.Domain.Repositories;

namespace AccountStatements.Application.Features.Statements.Commands.GenerateStatements
{
    public record GenerateStatementsCommand(string Month) : IRequest<GenerateStatementsResponse>;

    public record GenerateStatementsResponse(string Message, int GeneratedCount);

    public class GenerateStatementsCommandValidator : AbstractValidator<GenerateStatementsCommand>
    {
        public GenerateStatementsCommandValidator()
        {
            RuleFor(v => v.Month)
                .NotEmpty().WithMessage("Month is required.")
                .Matches(@"^\d{4}-(0[1-9]|1[0-2])$")
                .WithMessage("Month must be in YYYY-MM format (e.g., 2026-06).");
        }
    }

    public class GenerateStatementsCommandHandler : IRequestHandler<GenerateStatementsCommand, GenerateStatementsResponse>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountStatementRepository _accountStatementRepository;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IUnitOfWork _unitOfWork;

        public GenerateStatementsCommandHandler(
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

        public async Task<GenerateStatementsResponse> Handle(GenerateStatementsCommand request, CancellationToken cancellationToken)
        {

            var parts = request.Month.Split('-');
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);

            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1).AddTicks(-1);

            int generatedCount = 0;
            int pageNumber = 1;
            const int pageSize = 100;
            List<Customer> customers;

            do
            {
                customers = await _customerRepository.GetActivePagedAsync(pageNumber, pageSize, cancellationToken);

                foreach (var customer in customers)
                {
                    var exists = await _accountStatementRepository.ExistsAsync(customer.Id, request.Month, cancellationToken);
                    if (exists)
                    {
                        continue; 
                    }

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

                    generatedCount++;
                }

                pageNumber++;
            } while (customers.Count == pageSize);

            string message = generatedCount == 0 
                ? "No new statements were generated (they may already exist)." 
                : $"Successfully enqueued statements generation and emailing for {generatedCount} customer(s).";

            return new GenerateStatementsResponse(message, generatedCount);
        }
    }
}
