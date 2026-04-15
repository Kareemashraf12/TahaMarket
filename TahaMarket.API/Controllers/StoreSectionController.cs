using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services;

[ApiController]
[Route("api/store-sections")]
public class StoreSectionController : ControllerBase
{
    private readonly StoreSectionService _service;

    public StoreSectionController(StoreSectionService service)
    {
        _service = service;
    }

    // =========================================
    // CREATE SECTION (ADMIN ONLY)
    // =========================================
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateStoreSectionRequest request)
    {
        var result = await _service.Create(request);

        return Ok(new
        {
            message = "Section created successfully",
            data = result
        });
    }

    // =========================================
    // GET ALL SECTIONS (PUBLIC - HOME PAGE)
    // =========================================
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAll();

        return Ok(new
        {
            message = "Sections retrieved successfully",
            data = result
        });
    }

    // =========================================
    // GET BY ID (OPTIONAL)
    // =========================================
    [AllowAnonymous]
    [HttpGet("GetById/{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetById(id);

        return Ok(new
        {
            message = "Section retrieved successfully",
            data = result
        });
    }

    // =========================================
    // DELETE SECTION (ADMIN ONLY)
    // =========================================
    [Authorize(Roles = "Admin")]
    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.Delete(id);

        return Ok(new
        {
            message = "Section deleted successfully"
        });
    }
}