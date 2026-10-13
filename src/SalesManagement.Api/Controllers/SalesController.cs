using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.Common.Models;
using SalesManagement.Application.DTOs.Sales;
using SalesManagement.Application.Features.Sales.Commands.CancelSale;
using SalesManagement.Application.Features.Sales.Commands.CreateSale;
using SalesManagement.Application.Features.Sales.Queries.GetSaleById;
using SalesManagement.Application.Features.Sales.Queries.GetSales;

namespace SalesManagement.Api.Controllers;

[Authorize]
public class SalesController : ApiControllerBase
{
    /// <summary>
    /// Gets a paginated list of sales with optional filters by customer or seller.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SaleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSalesQuery(customerId, userId, pageNumber, pageSize);
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets detailed sale information including all line items and customer/seller info.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetSaleByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new sales order and deducts inventory stock automatically.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SaleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();

        var command = new CreateSaleCommand(
            request.CustomerId,
            currentUserId,
            request.Items);

        var result = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Cancels a sale and restores product inventory stock. Requires Admin role.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelSaleCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
