using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Services.KatoInfoService;

public interface IKatoInfoService
{
     Task<KatoResulDto> GetKatoInfoResult(string code);
     bool CheckFile(IFormFile? formFile);
     Task<Dictionary<string, Dictionary<string, string>>> GetInfoFromFile(IFormFile formFile);
     Task Delete(Dictionary<string, Dictionary<string, string>> katoFileInfo);
     Task Update(Dictionary<string, Dictionary<string, string>> katoFileInfo);
     Task Create(Dictionary<string, Dictionary<string, string>> katoFileInfo);
     Task UpdateLocality(Dictionary<string, Dictionary<string, string>> katoFileInfo);

}