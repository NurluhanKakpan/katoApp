using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;


[Route("api/[controller]")]
[ApiController]
public class KatoController : Controller
{
    private readonly IKatoInfoService _katoInfoService;

    public KatoController(IKatoInfoService katoInfoService)
    {
        _katoInfoService = katoInfoService;
    }

    [HttpPost]
    [ProducesResponseType(200, Type = typeof(KatoResulDto))]
    public async Task<IActionResult> GetResult([FromBody] KatoInputDto katoInputDto)
    {
        var result = await _katoInfoService.GetKatoInfoResult(katoInputDto.Code!);
        return Ok(result);
    }
}