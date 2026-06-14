using AccountStatements.Application.DTOs;
using AccountStatements.Application.Interfaces;
using AccountStatements.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace AccountStatements.Infrastructure.Jobs
{
    public class EmailJob : IEmailJob
    {
        private readonly IEmailService _emailService;
        private readonly ICustomerRepository _customerRepository;
        private readonly IAccountStatementRepository _statementRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmailJob> _logger;

        public EmailJob(
            IEmailService emailService,
            ICustomerRepository customerRepository,
            IAccountStatementRepository statementRepository,
            IUnitOfWork unitOfWork,
            ILogger<EmailJob> logger)
        {
            _emailService = emailService;
            _customerRepository = customerRepository;
            _statementRepository = statementRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task SendEmailAndUpdateStatusAsync(Guid statementId, AccountStatementDto statementDto, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing background email job for Statement ID: {StatementId}...", statementId);

            var statement = await _statementRepository.GetByIdAsync(statementId, cancellationToken);
            if (statement == null)
            {
                _logger.LogWarning("Statement {StatementId} not found in database. Email dispatch aborted.", statementId);
                return;
            }

            var customer = await _customerRepository.GetByIdAsync(statement.CustomerId, cancellationToken);
            if (customer == null)
            {
                _logger.LogWarning("Customer {CustomerId} not found. Email dispatch aborted.", statement.CustomerId);
                statement.EmailSentStatus = "Failed";
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return;
            }

            var success = await _emailService.SendStatementEmailAsync(customer.Email, customer.Name, statementDto, cancellationToken);

            statement.EmailSentStatus = success ? "Sent" : "Failed";
            statement.SentAt = success ? DateTime.UtcNow : null;
            statement.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Finished processing background email job for Statement ID: {StatementId}. Status: {Status}", statementId, statement.EmailSentStatus);
        }
    }
}
