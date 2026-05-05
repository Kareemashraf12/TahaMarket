using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TahaMarket.Application.DTOs;

namespace TahaMarket.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/addons")]
    public class AdminAddOnController : ControllerBase
    {
        private readonly AddOnService _service;

        public AdminAddOnController(AddOnService service)
        {
            _service = service;
        }

        // =========================
        // CREATE
        // =========================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAddOnGroupWithOptionsRequest request)
        {
            var id = await _service.CreateGroupWithOptions(request);
            return Ok(new { groupId = id });
        }



        // =========================
        // GET ONE (UNIFIED)
        // =========================
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] Guid? productId,[FromQuery] Guid? storeId)
        {
            var result = await _service.Get(productId, storeId);
            return Ok(new { addOns = result });
        }


        // =========================
        // DELETE GROUP
        // =========================
        [HttpDelete("DeleteGroup/{groupId}")]
        public async Task<IActionResult> DeleteGroup(Guid groupId)
        {
            await _service.DeleteGroup(groupId);
            return Ok(new { message = "Deleted successfully" });
        }

        // =========================
        // DELETE OPTION
        // =========================
        [HttpDelete("DeleteOption/{optionId}")] 
        public async Task<IActionResult> DeleteOption(Guid optionId)
        {
            await _service.DeleteOption(optionId);
            return Ok(new { message = "Deleted successfully" });
        }

    }
}
