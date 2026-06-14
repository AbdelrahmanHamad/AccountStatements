using Microsoft.AspNetCore.Mvc;
using AccountStatements.Application.Features.Transactions.Commands.CreateTransaction;

namespace AccountStatements.Api.Controllers
{
    public class TransactionsController : ApiControllerBase
    {
        /// <summary>
        /// Create a new transaction for a customer
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/transactions
        ///     {
        ///        "customerId": "11111111-1111-1111-1111-111111111111",
        ///        "amount": 1250.50,
        ///        "description": "Salary Deposit",
        ///        "transactionDate": "2026-06-12T10:00:00"
        ///     }
        ///
        /// </remarks>
        /// <param name="command">The transaction details</param>
        /// <returns>The GUID of the created transaction</returns>
        /// <response code="200">Returns the GUID of the created transaction</response>
        /// <response code="400">If validation fails or customer does not exist</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateTransactionCommand command)
        {
            var id = await Mediator.Send(command);
            return Ok(id);
        }
    }
}
