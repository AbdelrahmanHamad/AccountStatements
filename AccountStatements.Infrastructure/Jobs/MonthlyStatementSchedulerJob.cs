using System;
using System.Threading.Tasks;
using MediatR;
using AccountStatements.Application.Features.Statements.Commands.GenerateStatements;
using Microsoft.Extensions.Logging;

namespace AccountStatements.Infrastructure.Jobs
{
    public interface IMonthlyStatementSchedulerJob
    {
        Task RunMonthlyGenerationAsync();
    }

    public class MonthlyStatementSchedulerJob : IMonthlyStatementSchedulerJob
    {
        private readonly ISender _sender;
        private readonly ILogger<MonthlyStatementSchedulerJob> _logger;

        public MonthlyStatementSchedulerJob(ISender sender, ILogger<MonthlyStatementSchedulerJob> logger)
        {
            _sender = sender;
            _logger = logger;
        }

        public async Task RunMonthlyGenerationAsync()
        {
            string previousMonth = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM"); 
            var response = await _sender.Send(new GenerateStatementsCommand(previousMonth));
            
            _logger.LogInformation("Automatic Monthly Statement Scheduler finished. Result: {Message}, Generated Count: {Count}", 
                response.Message, response.GeneratedCount);
        }
    }
}
