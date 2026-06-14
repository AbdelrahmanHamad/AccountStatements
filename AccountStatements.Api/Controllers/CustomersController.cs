using Microsoft.AspNetCore.Mvc;
using AccountStatements.Application.Features.Customers.Commands.CreateCustomer;

namespace AccountStatements.Api.Controllers
{
    public class CustomersController : ApiControllerBase
    {
        /// <summary>
        /// Create a new customer
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/customers
        ///     {
        ///        "name": "Alice Johnson",
        ///        "email": "alice.johnson@example.com"
        ///     }
        /// 
        /// Sample response:
        /// 
        ///     "3fa85f64-5717-4562-b3fc-2c963f66afa6"
        /// 
        /// </remarks>
        /// <param name="command">The customer creation details</param>
        /// <response code="200">Returns the GUID of the created customer</response>
        /// <response code="400">If the request name or email is invalid</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command)
        {
            var id = await Mediator.Send(command);
            return Ok(id);
        }
    }
}
