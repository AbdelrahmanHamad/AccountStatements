using Microsoft.AspNetCore.Mvc;
using AccountStatements.Application.DTOs;
using AccountStatements.Application.Features.Statements.Commands.GenerateCustomerStatement;
using AccountStatements.Application.Features.Statements.Commands.GenerateStatements;
using AccountStatements.Application.Features.Statements.Queries.GetStatementById;
using AccountStatements.Application.Features.Statements.Queries.GetStatements;

namespace AccountStatements.Api.Controllers
{
    public class StatementsController : ApiControllerBase
    {
        /// <summary>
        /// Trigger monthly statements generation for all active customers
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/statements/generate
        ///     {
        ///        "month": "2026-06"
        ///     }
        /// 
        /// Sample response:
        /// 
        ///     {
        ///        "message": "Successfully generated and processed statements for 3 customer(s).",
        ///        "generatedCount": 3
        ///     }
        /// 
        /// </remarks>
        /// <param name="command">The target month to generate statements for (YYYY-MM)</param>
        /// <returns>Summary of the generation process</returns>
        /// <response code="200">Returns details of the generated statements count</response>
        /// <response code="400">If the input month format is invalid</response>
        [HttpPost("generate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GenerateStatementsResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Generate([FromBody] GenerateStatementsCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Trigger statement generation and emailing for a single customer
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/statements/generate/customer
        ///     {
        ///        "customerId": "11111111-1111-1111-1111-111111111111",
        ///        "month": "2026-06"
        ///     }
        /// 
        /// </remarks>
        /// <param name="command">The target customer and month to generate statement for</param>
        /// <returns>Summary of the customer statement generation</returns>
        /// <response code="200">If statement generation was enqueued successfully</response>
        /// <response code="400">If customer already has a statement or request validation fails</response>
        /// <response code="404">If customer was not found</response>
        [HttpPost("generate/customer")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GenerateCustomerStatementResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GenerateForCustomer([FromBody] GenerateCustomerStatementCommand command)
        {
            var result = await Mediator.Send(command);
            if (!result.Success)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new { Message = result.Message });
                }
                return BadRequest(new { Message = result.Message });
            }
            return Ok(result);
        }

        /// <summary>
        /// Fetch monthly statements filtered by Customer ID and/or Month
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/statements?customerId=11111111-1111-1111-1111-111111111111&amp;month=2026-06
        /// 
        /// Sample response:
        /// 
        ///     [
        ///       {
        ///         "id": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///         "customerId": "11111111-1111-1111-1111-111111111111",
        ///         "customerName": "Alice Johnson",
        ///         "customerEmail": "alice.johnson@example.com",
        ///         "statementMonth": "2026-06",
        ///         "startingBalance": 4829.50,
        ///         "endingBalance": 9454.25,
        ///         "generatedAt": "2026-06-12T19:30:00Z",
        ///         "emailSentStatus": "Sent",
        ///         "sentAt": "2026-06-12T19:30:05Z",
        ///         "transactions": []
        ///       }
        ///     ]
        /// 
        /// </remarks>
        /// <param name="customerId">Optional Customer GUID filter</param>
        /// <param name="month">Optional Month filter (YYYY-MM format, e.g., 2026-06)</param>
        /// <returns>A list of matching account statements</returns>
        /// <response code="200">Returns a list of matching account statements</response>
        /// <response code="400">If query parameters validation fails</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AccountStatementDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetStatements([FromQuery] Guid? customerId, [FromQuery] string? month)
        {
            var query = new GetStatementsQuery(customerId, month);
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Retrieve a detailed account statement by its unique ID
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/statements/8fa85f64-5717-4562-b3fc-2c963f66afa6
        /// 
        /// Sample response:
        /// 
        ///     {
        ///       "id": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "customerId": "11111111-1111-1111-1111-111111111111",
        ///       "customerName": "Alice Johnson",
        ///       "customerEmail": "alice.johnson@example.com",
        ///       "statementMonth": "2026-06",
        ///       "startingBalance": 4829.50,
        ///       "endingBalance": 9454.25,
        ///       "generatedAt": "2026-06-12T19:30:00Z",
        ///       "emailSentStatus": "Sent",
        ///       "sentAt": "2026-06-12T19:30:05Z",
        ///       "transactions": [
        ///         {
        ///           "id": "7fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///           "customerId": "11111111-1111-1111-1111-111111111111",
        ///           "amount": 5000.00,
        ///           "description": "Salary Deposit",
        ///           "transactionDate": "2026-06-01T09:00:00Z"
        ///         },
        ///         {
        ///           "id": "6fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///           "customerId": "11111111-1111-1111-1111-111111111111",
        ///           "amount": -300.00,
        ///           "description": "Electronics Store",
        ///           "transactionDate": "2026-06-05T11:00:00Z"
        ///         }
        ///       ]
        ///     }
        /// 
        /// </remarks>
        /// <param name="id">The statement GUID</param>
        /// <returns>The detailed account statement containing the list of transaction items</returns>
        /// <response code="200">Returns the detailed account statement</response>
        /// <response code="404">If the statement is not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountStatementDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var query = new GetStatementByIdQuery(id);
            var result = await Mediator.Send(query);
            if (result == null)
            {
                return NotFound(new { Message = $"Account statement with ID '{id}' was not found." });
            }
            return Ok(result);
        }
    }
}
