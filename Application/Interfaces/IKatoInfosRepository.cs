using Domain.Models;
namespace Application.Interfaces;

public interface IKatoInfoRepository
{
     Task<KatoInfo> GetKatoByCode(string code);
     Task<string> GetNameByCode(string code);
     Task<string> GetCodeByName(string name);
     Task<List<string>> GetCodesFromDb();
     Task<bool> CodeExists(string code);
     
     /*для файла*/
     Task<bool> Delete(string code);
     Task<bool> Create(string code,Dictionary<string,string> name);
     Task<bool> UpdateName(string code,string name);
     Task<bool> UpdateLocality(string code);
}