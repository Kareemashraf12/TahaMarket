using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TahaMarket.Application.DTOs;

[ApiController]
[Route("api/user-address")]
[Authorize(Roles = "Customer")] 
public class UserAddressController : ControllerBase
{
    private readonly UserAddressService _service;

    public UserAddressController(UserAddressService service)
    {
        _service = service;
    }

    // =========================
    // ADD ADDRESS
    // =========================
    [HttpPost]
    public async Task<IActionResult> Add(CreateAddressRequest request)
    {
        var result = await _service.AddAddress(request);
        return Ok(result);
    }

    // =========================
    // GET ALL
    // =========================
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _service.GetMyAddresses();
        return Ok(result);
    }

    // =========================
    // UPDATE
    // =========================
    [HttpPut]
    public async Task<IActionResult> Update(UpdateAddressRequest request)
    {
        var result = await _service.UpdateAddress(request);
        return Ok(result);
    }

    // =========================
    // DELETE
    // =========================
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAddress(id);
        return Ok(new { message = "Deleted successfully" });
    }
}