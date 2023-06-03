using Application.DTOs;

namespace Application.Services;

public interface IKatoInfoService
{
     Task<KatoResulDto> GetKatoInfoResult(string code);
}