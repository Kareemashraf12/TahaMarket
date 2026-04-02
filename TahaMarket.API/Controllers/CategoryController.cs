using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TahaMarket.Application.Services;
using TahaMarket.Domain.Entities;

[Authorize(Roles = "Store")]
[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly CategoryService _service;

    public CategoryController(CategoryService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(string name)
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        return Ok(await _service.Create(storeId, name));
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var storeId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        return Ok(await _service.GetMy(storeId));
    }
}