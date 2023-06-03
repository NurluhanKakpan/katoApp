using Domain.Models;

namespace Application;

public interface IKatoInfoRepository
{
     Task<KatoInfo> GetKatoByCode(string code);
     Task<string> GetNameByCode(string code);
     Task<string> GetCodeByName(string name);
     Task<List<string>> GetCodesFromDb();
     Task<bool> CodeExists(string code);
}