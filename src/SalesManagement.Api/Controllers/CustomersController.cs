using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.Common.Models;
using SalesManagement.Application.DTOs.Customers;
using SalesManagement.Application.Features.Customers.Commands.CreateCustomer;
using SalesManagement.Application.Features.Customers.Commands.DeleteCustomer;
using SalesManagement.Application.Features.Customers.Commands.UpdateCustomer;
using SalesManagement.Application.Features.Customers.Queries.GetCustomerById;
using SalesManagement.Application.Features.Customers.Queries.GetCustomers;

namespace SalesManagement.Api.Controllers;

[Authorize]
public class CustomersController : ApiControllerBase
{
    /// <summary>
    /// Gets a paginated list of customers, with optional search filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCustomersQuery(searchTerm, pageNumber, pageSize);
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets customer details by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCustomerByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(
            request.Name,
            request.Email,
            request.Phone,
            request.DocumentNumber,
            request.Address);

        var result = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(
            id,
            request.Name,
            request.Email,
            request.Phone,
            request.DocumentNumber,
            request.Address,
            request.IsActive);

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deactivates a customer. Requires Admin role.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteCustomerCommand(id);
        await Mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
