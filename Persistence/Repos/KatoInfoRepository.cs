using System.Text.Json;
using Application.Interfaces;
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
    
    /* для файла */
    
    public async Task<bool> Delete(string code)
    {
        var result = await _dataContext.KatoInfos.FirstOrDefaultAsync(q => q.Code == code);
        result!.DeletedAt = DateTime.Now.ToUniversalTime();
        result.UpdatedAt = DateTime.Now.ToUniversalTime();
        result.IsDeleted = true;
        return await _dataContext.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> Create(string code, Dictionary<string, string> name)
    {
        var options = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false
        };
        var newNameForDb = JsonSerializer.Serialize(name,options);
        var katoInfo = new KatoInfo
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.Now.ToUniversalTime(),
            IsDeleted = false,
            Code = code,
            Name = newNameForDb,
            IsMain = false,
            IsLocality = false
        };
        await _dataContext.KatoInfos.AddAsync(katoInfo);
        return await _dataContext.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> UpdateName(string code, string name)
    {
        var result = await _dataContext.KatoInfos.FirstOrDefaultAsync(q => q.Code == code);
        result!.Name = name;
        result.UpdatedAt = DateTime.Now.ToUniversalTime();
        return await _dataContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateLocality(string code)
    {
        var result = await _dataContext.KatoInfos.FirstOrDefaultAsync(q => q.Code == code);
        result!.IsLocality = true;
        return await _dataContext.SaveChangesAsync() > 0;
    }
}