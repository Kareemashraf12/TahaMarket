using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TahaMarket.Application.Services;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly CategoryService _service;

    public CategoryController(CategoryService service)
    {
        _service = service;
    }

    // =========================
    //  STORE: Create Category
    // =========================
    [Authorize(Roles = "Store")]
    [HttpPost]
    public async Task<IActionResult> CreateForStore([FromBody] string name)
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.Create(storeId, name);

        return Ok(new
        {
            message = "Category created successfully",
            data = result
        });
    }

    // =========================
    //   ADMIN: Create Category for any Store
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpPost("admin")]
    public async Task<IActionResult> CreateForAnyStore(Guid storeId, [FromBody] string name)
    {
        var result = await _service.Create(storeId, name);

        return Ok(new
        {
            message = "Category created for store",
            data = result
        });
    }

    // =========================
    //  STORE: Get My Categories
    // =========================
    [Authorize(Roles = "Store")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyCategories()
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        var result = await _service.GetMy(storeId);

        return Ok(result);
    }

    // =========================
    // PUBLIC: Get Categories by Store
    // =========================
    [AllowAnonymous]
    [HttpGet("public/{storeId}")]
    public async Task<IActionResult> GetByStore(Guid storeId)
    {
        var result = await _service.GetByStore(storeId);

        return Ok(result);
    }

    // =========================
    //  ADMIN: Get Categories for Store
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/{storeId}")]
    public async Task<IActionResult> GetForStore(Guid storeId)
    {
        var result = await _service.GetByStore(storeId);

        return Ok(result);
    }
}