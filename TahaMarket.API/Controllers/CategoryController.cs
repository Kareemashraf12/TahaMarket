using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    // ADMIN: Create Category
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(Guid storeId, [FromBody] string name)
    {
        var result = await _service.Create(storeId, name);

        return Ok(new
        {
            message = "Category created successfully",
            data = result
        });
    }

    // =========================
    // PUBLIC: Get Categories by Store
    // =========================
    [AllowAnonymous]
    [HttpGet("{storeId}")]
    public async Task<IActionResult> GetByStore(Guid storeId)
    {
        var result = await _service.GetByStore(storeId);

        return Ok(result);
    }

    // =========================
    // PUBLIC: Get Category by Id
    // =========================
    [AllowAnonymous]
    [HttpGet("details/{categoryId}")]
    public async Task<IActionResult> GetById(Guid categoryId)
    {
        var result = await _service.GetById(categoryId);

        return Ok(result);
    }
}