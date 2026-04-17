using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TahaMarket.Application.DTOs;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly ProductService _service;

    public ProductController(ProductService service)
    {
        _service = service;
    }

    // =========================
    // CREATE PRODUCT (Admin Only)
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] CreateProductRequest request)
    {
        var result = await _service.Create(request.StoreId, request);
        return Ok(result);
    }

    // =========================
    // GET ALL PRODUCTS (HOME - PUBLIC)
    // =========================
    [AllowAnonymous]
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _service.GetAllProducts(request);
        return Ok(result);
    }

    // =========================
    // GET DETAILS (PUBLIC)
    // =========================
    [AllowAnonymous]
    [HttpGet("GetDetails/{id}")]
    public async Task<IActionResult> GetDetails(Guid id)
    {
        var result = await _service.GetDetails(id);
        return Ok(result);
    }

    // =========================
    // UPDATE PRODUCT (Admin Only)
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpPut("Update/{id}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateProductRequest request)
    {
        var storeId = Guid.Parse(User.FindFirst("StoreId").Value);

        await _service.Update(id, storeId, request);

        return Ok(new
        {
            message = "Product updated successfully"
        });
    }

    // =========================
    // DELETE PRODUCT (Admin Only)
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var storeId = Guid.Parse(User.FindFirst("StoreId").Value);

        await _service.Delete(id, storeId);

        return Ok(new
        {
            message = "Product deleted successfully"
        });
    }

    [AllowAnonymous]
    [HttpGet("by-category/{categoryId}")]
    public async Task<IActionResult> GetByCategory(
    Guid categoryId,
    [FromQuery] PaginationRequest request)
    {
        var result = await _service.GetByCategory(categoryId, request);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("top-selling")]
    public async Task<IActionResult> GetTopSellingProducts([FromQuery] int take = 10)
    {
        var result = await _service.GetTopSellingProducts(take);
        return Ok(result);
    }

    //  Get Hot Offers Products
    [AllowAnonymous]
    [HttpGet("hot-offers")]
    public async Task<IActionResult> GetHotOffers()
    {
        var result = await _service.GetHotOffers();
        return Ok(result);
    }
}