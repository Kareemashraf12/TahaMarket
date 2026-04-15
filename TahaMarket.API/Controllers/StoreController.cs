using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services;

[ApiController]
[Route("api/stores")]
public class StoreController : ControllerBase
{
    private readonly StoreService _service;

    public StoreController(StoreService service)
    {
        _service = service;
    }

    // =========================
    // ADMIN: Create Store
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] CreateStoreRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _service.CreateStore(request);

        return Ok(new
        {
            message = "Store created successfully",
            data = result
        });
    }

    // =========================
    // ADMIN: Update Store
    // =========================
    [Authorize(Roles = "Admin,Store")]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateStoreRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _service.UpdateStore(id, request);

        return Ok(new
        {
            message = "Store updated successfully"
        });
    }

    // =========================
    // PUBLIC: Get All Stores
    // =========================
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var stores = await _service.GetAll();
        return Ok(stores);
    }

    // =========================
    // PUBLIC: Get Store Details
    // =========================
    [AllowAnonymous]
    [HttpGet("GetDetailsById/{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var store = await _service.GetById(id);
        return Ok(store);
    }

    // =========================
    // ADMIN: Delete Store
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteStore(id);

        return Ok(new
        {
            message = "Store deleted successfully"
        });
    }
}