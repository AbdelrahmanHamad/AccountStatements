using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using AccountStatements.Application.DTOs;
using AccountStatements.Domain.Repositories;

namespace AccountStatements.Application.Features.Statements.Queries.GetStatements
{
    public record GetStatementsQuery(Guid? CustomerId, string? Month) : IRequest<List<AccountStatementDto>>;

    public class GetStatementsQueryValidator : AbstractValidator<GetStatementsQuery>
    {
        public GetStatementsQueryValidator()
        {
            RuleFor(v => v.Month)
                .Matches(@"^\d{4}-(0[1-9]|1[0-2])$")
                .WithMessage("Month must be in YYYY-MM format (e.g., 2026-06).")
                .When(v => !string.IsNullOrEmpty(v.Month));
        }
    }

    public class GetStatementsQueryHandler : IRequestHandler<GetStatementsQuery, List<AccountStatementDto>>
    {
        private readonly IAccountStatementRepository _accountStatementRepository;

        public GetStatementsQueryHandler(IAccountStatementRepository accountStatementRepository)
        {
            _accountStatementRepository = accountStatementRepository;
        }

        public async Task<List<AccountStatementDto>> Handle(GetStatementsQuery request, CancellationToken cancellationToken)
        {
            var statements = await _accountStatementRepository.GetStatementsAsync(request.CustomerId, request.Month, cancellationToken);

            return statements.Select(s => new AccountStatementDto
            {
                Id = s.Id,
                CustomerId = s.CustomerId,
                CustomerName = s.Customer.Name,
                CustomerEmail = s.Customer.Email,
                StatementMonth = s.StatementMonth,
                StartingBalance = s.StartingBalance,
                EndingBalance = s.EndingBalance,
                GeneratedAt = s.GeneratedAt,
                EmailSentStatus = s.EmailSentStatus,
                SentAt = s.SentAt
            }).ToList();
        }
    }
}
