using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.Common.Models;
using SalesManagement.Application.DTOs.Products;
using SalesManagement.Application.Features.Products.Commands.CreateProduct;
using SalesManagement.Application.Features.Products.Commands.DeleteProduct;
using SalesManagement.Application.Features.Products.Commands.UpdateProduct;
using SalesManagement.Application.Features.Products.Commands.UpdateStock;
using SalesManagement.Application.Features.Products.Queries.GetProductById;
using SalesManagement.Application.Features.Products.Queries.GetProducts;

namespace SalesManagement.Api.Controllers;

[Authorize]
public class ProductsController : ApiControllerBase
{
    /// <summary>
    /// Gets a paginated list of catalog products, with optional search filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsQuery(searchTerm, pageNumber, pageSize);
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets product details by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new product in the catalog. Requires Admin role.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.Sku,
            request.Price,
            request.StockQuantity);

        var result = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates product information. Requires Admin role.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.Sku,
            request.Price,
            request.IsActive);

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates stock level for a product. Requires Admin role.
    /// </summary>
    [HttpPatch("{id:guid}/stock")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateStockCommand(id, request.StockQuantity);
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deactivates a product. Requires Admin role.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand(id);
        await Mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
