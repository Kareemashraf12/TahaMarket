using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TahaMarket.Application.DTOs;
using TahaMarket.Application.Services;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class StoreController : ControllerBase
{
    private readonly StoreService _service;

    public StoreController(StoreService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateStoreRequest request)
    {
        var result = await _service.CreateStore(request);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var stores = await _service.GetAll();
        return Ok(stores);
    }
}