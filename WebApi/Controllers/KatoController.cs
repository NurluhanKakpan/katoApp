using Application.DTOs;
using Application.Services.KatoInfoService;
using Application.Validators;
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

    
    
    /* для файла*/
    
    [HttpPost("/upload")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> UploadFile([FromForm] FileDto? fileDto)
    {
        if (!_katoInfoService.CheckFile(fileDto!.File))
        {
            return Ok("Try again");
        }

        if (!FileValidator.ValidateFile(fileDto.File!))
        {
            return Ok("Your input file format is wrong");
        }
        var katoInfoFromFile = await _katoInfoService.GetInfoFromFile(fileDto.File!);
        await _katoInfoService.Delete(katoInfoFromFile);
        await _katoInfoService.Update(katoInfoFromFile);
        await _katoInfoService.Create(katoInfoFromFile);
        await _katoInfoService.UpdateLocality(katoInfoFromFile);
        return Ok("Successfully");
    }

}