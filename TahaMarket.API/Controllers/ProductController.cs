using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services;

[Authorize(Roles = "Store")]
[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly ProductService _service;

    public ProductController(ProductService service)
    {
        _service = service;
    }

    // Create
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateProductRequest request)
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        return Ok(await _service.Create(storeId, request));
    }

    // Get ALL products for store 
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        return Ok(await _service.GetAllByStore(storeId));
    }

    // Get By Category
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetByCategory(Guid categoryId)
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        return Ok(await _service.GetByCategory(storeId, categoryId));
    }

    // Details
    [HttpGet("{id}")]
    public async Task<IActionResult> Details(Guid id)
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        return Ok(await _service.GetDetails(id, storeId));
    }

    // Update
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateProductRequest request)
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        await _service.Update(id, storeId, request);
        return Ok("Updated successfully");
    }

    // Delete
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        await _service.Delete(id, storeId);
        return Ok("Deleted successfully");
    }
}