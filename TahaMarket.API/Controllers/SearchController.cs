using Microsoft.AspNetCore.Mvc;
using TahaMarket.Application.DTOs;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly SearchService _service;

    public SearchController(SearchService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchRequest request)
    {
        var result = await _service.Search(request);
        return Ok(result);
    }
}