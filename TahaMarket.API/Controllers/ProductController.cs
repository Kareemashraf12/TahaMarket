using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services;

[ApiController]
[Route("api/products")]
[Authorize(Roles = "Store,Admin")]
public class ProductController : ControllerBase
{
    private readonly ProductService _service;

    public ProductController(ProductService service)
    {
        _service = service;
    }

    // =========================================================
    // STORE → Create Product (for himself)
    // =========================================================
    [Authorize(Roles = "Store")]
    [HttpPost("store/create")]
    public async Task<IActionResult> CreateForStore([FromForm] CreateProductRequest request)
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.Create(storeId, request);

        return Ok(new
        {
            message = "Product created successfully",
            data = result
        });
    }

    // =========================================================
    // ADMIN → Create Product for ANY store
    // =========================================================
    [Authorize(Roles = "Admin")]
    [HttpPost("admin/create")]
    public async Task<IActionResult> CreateForAnyStore(
        [FromQuery] Guid storeId,
        [FromForm] CreateProductRequest request)
    {
        if (storeId == Guid.Empty)
            return BadRequest("StoreId is required");

        var result = await _service.Create(storeId, request);

        return Ok(new
        {
            message = "Product created for store",
            data = result
        });
    }

    // =========================================================
    // STORE → Get My Products
    // =========================================================
    [Authorize(Roles = "Store")]
    [HttpGet("store/my")]
    public async Task<IActionResult> GetMyProducts()
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.GetAllByStore(storeId);

        return Ok(new
        {
            message = "Store products retrieved",
            data = result
        });
    }

    // =========================================================
    // ADMIN → Get Products for specific store
    // =========================================================
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/store/{storeId}")]
    public async Task<IActionResult> GetProductsByStore(Guid storeId)
    {
        var result = await _service.GetAllByStore(storeId);

        return Ok(new
        {
            message = "Store products retrieved (Admin)",
            data = result
        });
    }

    // =========================================================
    // PUBLIC → Get All Products (Guest / User)
    // =========================================================
    [AllowAnonymous]
    [HttpGet("public")]
    public async Task<IActionResult> GetAllPublic()
    {
        var result = await _service.GetAllProducts();

        return Ok(new
        {
            message = "All products retrieved",
            data = result
        });
    }

    // =========================================================
    // PUBLIC → Get Products by Store
    // =========================================================
    [AllowAnonymous]
    [HttpGet("public/store/{storeId}")]
    public async Task<IActionResult> GetByStore(Guid storeId)
    {
        var result = await _service.GetAllByStore(storeId);

        return Ok(new
        {
            message = "Store products retrieved",
            data = result
        });
    }

    // =========================================================
    // STORE → Get Products by Category (own store)
    // =========================================================
    [Authorize(Roles = "Store")]
    [HttpGet("store/category/{categoryId}")]
    public async Task<IActionResult> GetByCategoryForStore(Guid categoryId)
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.GetByCategory(storeId, categoryId);

        return Ok(new
        {
            message = "Products by category retrieved",
            data = result
        });
    }

    // =========================================================
    // ADMIN → Get Products by Category for any store
    // =========================================================
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/store/{storeId}/category/{categoryId}")]
    public async Task<IActionResult> GetByCategoryForAdmin(Guid storeId, Guid categoryId)
    {
        var result = await _service.GetByCategory(storeId, categoryId);

        return Ok(new
        {
            message = "Products by category retrieved (Admin)",
            data = result
        });
    }

    // =========================================================
    // STORE → Get Product Details
    // =========================================================
    [Authorize(Roles = "Store")]
    [HttpGet("store/{id}")]
    public async Task<IActionResult> GetDetailsForStore(Guid id)
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.GetDetails(id, storeId);

        return Ok(new
        {
            message = "Product details retrieved",
            data = result
        });
    }

    // =========================================================
    // ADMIN → Get Product Details for any store
    // =========================================================
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/store/{storeId}/product/{id}")]
    public async Task<IActionResult> GetDetailsForAdmin(Guid storeId, Guid id)
    {
        var result = await _service.GetDetails(id, storeId);

        return Ok(new
        {
            message = "Product details retrieved (Admin)",
            data = result
        });
    }

    // =========================================================
    // STORE → Update Product
    // =========================================================
    [Authorize(Roles = "Store")]
    [HttpPut("store/update/{id}")]
    public async Task<IActionResult> UpdateForStore(
        Guid id,
        [FromForm] UpdateProductRequest request)
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        await _service.Update(id, storeId, request);

        return Ok(new
        {
            message = "Product updated successfully"
        });
    }

    // =========================================================
    // ADMIN → Update Product for any store
    // =========================================================
    [Authorize(Roles = "Admin")]
    [HttpPut("admin/store/{storeId}/update/{id}")]
    public async Task<IActionResult> UpdateForAdmin(
        Guid storeId,
        Guid id,
        [FromForm] UpdateProductRequest request)
    {
        await _service.Update(id, storeId, request);

        return Ok(new
        {
            message = "Product updated successfully (Admin)"
        });
    }

    // =========================================================
    // STORE → Delete Product
    // =========================================================
    [Authorize(Roles = "Store")]
    [HttpDelete("store/delete/{id}")]
    public async Task<IActionResult> DeleteForStore(Guid id)
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        await _service.Delete(id, storeId);

        return Ok(new
        {
            message = "Product deleted successfully"
        });
    }

    // =========================================================
    // ADMIN → Delete Product for any store
    // =========================================================
    [Authorize(Roles = "Admin")]
    [HttpDelete("admin/store/{storeId}/delete/{id}")]
    public async Task<IActionResult> DeleteForAdmin(Guid storeId, Guid id)
    {
        await _service.Delete(id, storeId);

        return Ok(new
        {
            message = "Product deleted successfully (Admin)"
        });
    }

    // =========================================================
    // PUBLIC → Get Random Products (for homepage)
    // =========================================================

    [AllowAnonymous]
    [HttpGet("public/random")]
    public async Task<IActionResult> GetRandomProducts(int count = 10)
    {
        var result = await _service.GetRandomProducts(count);
        return Ok(result);
    }


    // =========================================================
    // PUBLIC → Get All Products with Pagination
    // =========================================================
    [AllowAnonymous]
    [HttpGet("public/paginated")]
    public async Task<IActionResult> GetAllPaginated([FromQuery] PaginationRequest request)
    {
        var result = await _service.GetAllProducts(request);
        return Ok(result);
    }


    // =========================================================
    // PUBLIC → Get Filtered Products with Pagination
    // =========================================================
    [AllowAnonymous]
    [HttpGet("public/filter")]
    public async Task<IActionResult> GetFiltered(Guid? storeId,Guid? categoryId,
    [FromQuery] PaginationRequest request)
    {
        var result = await _service.GetFilteredProducts(storeId, categoryId, request);
        return Ok(result);
    }

    // =========================================================
    // PUBLIC → Search Products by Name or Description with Pagination
    // =========================================================
    [AllowAnonymous]
    [HttpGet("public/search")]
    public async Task<IActionResult> Search(
    string text,
    [FromQuery] PaginationRequest request)
    {
        return Ok(await _service.Search(text, request));
    }
}