using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
    [Authorize]
    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromForm] UpdateProductRequest request)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(role) || userIdClaim == null)
            return Unauthorized("Invalid token");

        var userId = Guid.Parse(userIdClaim.Value);

        // =========================
        // ADMIN
        // =========================
        if (role == "Admin")
        {
            if (request.StoreId == null)
                return BadRequest("StoreId required for admin");

            await _service.Update(request.ProductId, request.StoreId.Value, request);
        }

        // =========================
        // STORE OWNER
        // =========================
        else if (role == "Store")
        {
            
            await _service.Update(request.ProductId, userId, request);
        }

        
        else
        {
            return Forbid();
        }

        return Ok(new { message = "Updated successfully" });
    }
    // =========================
    // DELETE PRODUCT (Admin Only)
    // =========================
    [Authorize]
    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(role) || userIdClaim == null)
            return Unauthorized();

        Guid? storeId = null;

        // =========================
        // STORE
        // =========================
        if (role == "Store")
        {
            storeId = Guid.Parse(userIdClaim.Value);
        }

        // =========================
        // ADMIN → storeId = null
        // =========================

        await _service.Delete(id, storeId, role);

        return Ok(new { message = "Deleted successfully" });
    }


    // =========================
    // GET PRODUCTS BY CATEGORY (PUBLIC)
    // =========================
    [AllowAnonymous]
    [HttpGet("by-category/{categoryId}")]
    public async Task<IActionResult> GetByCategory(Guid categoryId,[FromQuery] PaginationRequest request)
    {
        var result = await _service.GetByCategory(categoryId, request);
        return Ok(result);
    }

    // =========================
    // GET TOP SELLING PRODUCTS (PUBLIC)
    // =========================
    [AllowAnonymous]
    [HttpGet("top-selling")]
    public async Task<IActionResult> GetTopSellingProducts([FromQuery] int take = 10)
    {
        var result = await _service.GetTopSellingProducts(take);
        return Ok(result);
    }

    // =========================
    // GET HOT OFFERS (PUBLIC)
    // =========================
    [AllowAnonymous]
    [HttpGet("hot-offers")]
    public async Task<IActionResult> GetHotOffers()
    {
        var result = await _service.GetHotOffers();
        return Ok(result);
    }


    // =========================
    // DAILY OFFERS
    // =========================
    [HttpGet("daily-offers")]
    public async Task<IActionResult> GetDailyOffers([FromQuery] PaginationRequest request)
    {
        if (request.Page <= 0)
            request.Page = 1;

        if (request.PageSize <= 0)
            request.PageSize = 10;

        if (request.PageSize > 50)
            request.PageSize = 50;

        var result = await _service.GetDailyOffers(request);

        return Ok(result);
    }
}