using Microsoft.AspNetCore.Mvc;
using AccountStatements.Application.Features.Seed.Commands.SeedData;

namespace AccountStatements.Api.Controllers
{
    public class SeedController : ApiControllerBase
    {
        /// <summary>
        /// Seed the database with sample customer and transaction data
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/seed
        /// 
        /// Sample response:
        /// 
        ///     "Database successfully seeded with 3 customers and 12 sample transactions."
        /// 
        /// </remarks>
        /// <response code="200">Returns completion status message</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        public async Task<IActionResult> Seed()
        {
            var message = await Mediator.Send(new SeedDataCommand());
            return Ok(message);
        }
    }
}