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
    //  ADMIN: Create Store
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpPost("CreateStoreByAdmin")]
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
    //  ADMIN: Get ALL Stores
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpGet("AdminGetAllStores")]
    public async Task<IActionResult> GetAll()
    {
        var stores = await _service.GetAll();

        return Ok(stores);
    }

    // =========================
    //  ADMIN: Get Store By Id
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpGet("AdmiGetStore/{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var store = await _service.GetById(id);

        if (store == null)
            return NotFound(new { message = "Store not found" });

        return Ok(store);
    }

    // =========================
    //  ADMIN: Update Store
    // =========================
    [Authorize(Roles = "Admin")]
    [HttpPut("AdminUpdateStore/{id}")]
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
    //  PUBLIC: Get All Stores
    // =========================
    [AllowAnonymous]
    [HttpGet("publicGetStores")]
    public async Task<IActionResult> GetAllPublic()
    {
        var stores = await _service.GetAll();

        return Ok(stores);
    }

    // =========================
    //  PUBLIC: Get Store Details
    // =========================
    [AllowAnonymous]
    [HttpGet("publicGetStoreDetails/{id}")]
    public async Task<IActionResult> GetPublicById(Guid id)
    {
        var store = await _service.GetById(id);

        if (store == null)
            return NotFound(new { message = "Store not found" });

        return Ok(store);
    }
}