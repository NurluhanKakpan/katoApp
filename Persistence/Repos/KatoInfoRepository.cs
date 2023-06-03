using Application;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repos;

public class KatoInfoRepository :IKatoInfoRepository
{
    private readonly DataContext _dataContext;

    public KatoInfoRepository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }
    public async Task<KatoInfo> GetKatoByCode(string code)
    {
        var result = await _dataContext.KatoInfos.FirstOrDefaultAsync(e => e.Code == code);
        return result!;
    }

    public async Task<string> GetNameByCode(string code)
    {
        var result = await _dataContext.KatoInfos.FirstOrDefaultAsync(e => e.Code == code);
        return result!.Name!;
    }

    public async Task<string> GetCodeByName(string name)
    {
        var result = await _dataContext.KatoInfos.FirstOrDefaultAsync(q => q.Name == name);
        return result!.Code!;
    }

    public async Task<List<string>> GetCodesFromDb()
    {
        var result = await _dataContext.KatoInfos.Where(q=>q.IsDeleted == false).Select(q => q.Code).ToListAsync();
        return result!;
    }

    public async Task<bool> CodeExists(string code)
    {
        var result = await _dataContext.KatoInfos.AnyAsync(e => e.Code == code);
        return result;
    }
}