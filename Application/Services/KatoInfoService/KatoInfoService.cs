using System.Text.Json;
using Application.DTOs;
using Application.Interfaces;
using Application.Validators;
using Microsoft.AspNetCore.Http;

namespace Application.Services.KatoInfoService;

public class KatoInfoService : IKatoInfoService
{
    private readonly IKatoInfoRepository _katoInfosRepository;

    public KatoInfoService(IKatoInfoRepository katoInfosRepository)
    {
        _katoInfosRepository = katoInfosRepository;
    }
    public async Task<KatoResulDto> GetKatoInfoResult(string code)
    {
        var validator = new KatoNumberValidator();
        var validatorResult = await validator.ValidateAsync(code);
        if (!validatorResult.IsValid)
        {
            return new KatoResulDto()
            {
                Code = code,
            };
        }
        if (!await _katoInfosRepository.CodeExists(code))
        {
            return new KatoResulDto()
            {
                Code = code,
            };
        }
        var katoInfoFromDb = await _katoInfosRepository.GetKatoByCode(code);
        var katoCodesFromDb = await _katoInfosRepository.GetCodesFromDb();
        katoCodesFromDb.Remove(code);
        var highLevel = await GetHighLevel(code, katoCodesFromDb);
        var listOfLowLevels = new List<LevelDto>();
        var filteredCodes = await GetLowLevelCodes(code, katoCodesFromDb);
        foreach (var t in filteredCodes)
        {
            var lowLevel = new LevelDto
            {
                Code = t,
                Name = await _katoInfosRepository.GetNameByCode(t)
            };
            listOfLowLevels.Add(lowLevel);
        }
        if (!IsLocality(code))
        {
            var katoResultFalseDto = new KatoResulDto
            {
                Code = code,
                IsLocality = false,
                Locality = null,
                Region = await _katoInfosRepository.GetNameByCode(string.Concat(code.AsSpan(0, 2), "0000000")),
                IsDeleted = (bool)katoInfoFromDb.IsDeleted!,
                HighLevel = highLevel,
                LowLevel = listOfLowLevels,
            };
            if (ParseForInt(code, 0, 2) < 70 && ParseForInt(code, 0, 2) >= 10 && ParseForInt(code, 2, 2) == 0)
            {
                katoResultFalseDto.District = null;
                katoResultFalseDto.HighLevel = null;
                return katoResultFalseDto;
            }
            if (ParseForInt(code,0,2) > 70 && ParseForInt(code,2,2) > 0)
            {
                katoResultFalseDto.District = await _katoInfosRepository.GetNameByCode(code);
                katoResultFalseDto.LowLevel = listOfLowLevels;
            }
            else
            {
                katoResultFalseDto.District = await _katoInfosRepository.GetNameByCode(string.Concat(code.AsSpan(0, 4), "00000"));
            }
            return katoResultFalseDto;
        }
        var katoResultTrueDto = new KatoResulDto
        {
            Code = code,
            IsLocality = true,
            Locality = await _katoInfosRepository.GetNameByCode(code),
            Region = await _katoInfosRepository.GetNameByCode(string.Concat(code.AsSpan(0, 2), "0000000")),
            District = await _katoInfosRepository.GetNameByCode(string.Concat(code.AsSpan(0, 4), "00000")),
            IsDeleted = (bool)katoInfoFromDb.IsDeleted!,
            HighLevel = highLevel,
            LowLevel = listOfLowLevels,
        };
        if (katoResultTrueDto.Region == katoResultTrueDto.Locality)
        {
            katoResultTrueDto.District = null;
            katoResultTrueDto.HighLevel = null;
        }
        return katoResultTrueDto;
    }
    private static bool IsLocality(string katoNumber)
    {
        /*Города республиканского значения*/
        if (ParseForInt(katoNumber,0,2) > 70 && ParseForInt(katoNumber,2,2) == 0)
        {
            return true;
        }
        /*Города областного значения*/
        if (ParseForInt(katoNumber,0,2) < 70 && ParseForInt(katoNumber,0,2) >=10 && 
            ParseForInt(katoNumber,2,2) >= 10 && ParseForInt(katoNumber,2,2) < 30 && 
            ParseForInt(katoNumber,4,2) == 10)
        {
            return true;
        }
        /*Села, поселки*/
        if (ParseForInt(katoNumber,0,2) < 70 && ParseForInt(katoNumber,0,2) >=10 && 
            ParseForInt(katoNumber,2,2) > 30 && ParseForInt(katoNumber,2,2) < 80 && 
            ParseForInt(katoNumber,4,2) > 30 && ParseForInt(katoNumber,6,2) >=10)
        {
            return true;
        }
        /*Села, поселки областного значения*/
        if (ParseForInt(katoNumber,4,2) == 0 && ParseForInt(katoNumber,6,2) >=10)
        {
            return true;
        }
        return false;
    }
    
    private static string GetHighLevelCode(string code, List<string> katoCodesFromDb)
    {
        var newCode = code.PadRight(9, '0');
        if (code.Length <= 2)
        {
            return newCode;
        }
        if (katoCodesFromDb.Contains(newCode))
        {
            return newCode;
        }
        return GetHighLevelCode(code.Substring(0, code.Length - 2), katoCodesFromDb);
    }
    
    private async Task<LevelDto> GetHighLevel(string code, List<string> katoCodesFromDb)
    {
        var highLevel = new LevelDto();
        if (ParseForInt(code, 7, 2) > 0)
        {
            highLevel.Code = code[..7] + "00";
            if (!katoCodesFromDb.Contains(highLevel.Code))
            {
                highLevel.Code = GetHighLevelCode(code[..8], katoCodesFromDb);
            }
            highLevel.Name = await _katoInfosRepository.GetNameByCode(highLevel.Code);
            return highLevel;
        }
        highLevel.Code = GetHighLevelCode(code[..8], katoCodesFromDb);
        highLevel.Name = await _katoInfosRepository.GetNameByCode(GetHighLevelCode(code[..8], katoCodesFromDb));
        return highLevel;
    }
    private async Task<List<string>> GetLowLevelCodes(string code, List<string> katoCodesFromDb)
    {
        if (ParseForInt(code,7,2) > 0)
        {
            return new List<string>();
        }
        var newCode = code.TrimEnd('0');
        var filteredCodes = katoCodesFromDb.Where(q => q.StartsWith(newCode)).ToList();
        var newFilteredCodes = new List<string>();
        for (var i = 0; i < filteredCodes.Count; i++)
        {
            katoCodesFromDb.Remove(filteredCodes[i]);
            katoCodesFromDb.Add(code);
            var highLevel = await GetHighLevel(filteredCodes[i], katoCodesFromDb);
            katoCodesFromDb.Add(filteredCodes[i]);
            katoCodesFromDb.Remove(code);
            if (highLevel.Code == code)
            {
                newFilteredCodes.Add(filteredCodes[i]);
            }
        }
        return newFilteredCodes;
    }
    private static int ParseForInt(string code, int start, int length)
    {
        return int.Parse(code.Substring(start, length));
    }
    
    
    /* для файла*/
    public bool CheckFile(IFormFile? formFile)
    {
        return formFile != null && formFile.Length != 0;
    }
    public async Task<Dictionary<string, Dictionary<string, string>>> GetInfoFromFile(IFormFile formFile)
    {
        var codesFromFile = new Dictionary<string,Dictionary<string,string>>();
        await using (var stream = formFile.OpenReadStream())
        {
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    var values = line!.Split(';');
                    var code = values[0];
                    var ruName = values[1];
                    var kzName = values[2];
                    var dictOfName = new Dictionary<string, string?>
                    {
                        { "Ru", ruName },
                        { "Kz", kzName },
                        { "En", null }
                    };
                    codesFromFile.Add(code,dictOfName!);
                }
            }
        }
        codesFromFile.Remove("code");
        return codesFromFile;
    }
    public async Task Delete(Dictionary<string, Dictionary<string, string>> katoFileInfo)
    {
        var codesFromDb = await _katoInfosRepository.GetCodesFromDb();
        foreach (var codeFromDb in codesFromDb.Where(codeFromDb => !katoFileInfo.ContainsKey(codeFromDb)))
        {
            await _katoInfosRepository.Delete(codeFromDb);
        }
    }
    public async Task Update(Dictionary<string, Dictionary<string, string>> katoFileInfo)
    {
        var codesFromDb = await _katoInfosRepository.GetCodesFromDb();
        foreach (var code in katoFileInfo.Keys)
        {
            if (codesFromDb.Contains(code))
            {
                var options = new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = false
                };
                var katoInfo = await _katoInfosRepository.GetKatoByCode(code);
                var nameInDict = katoFileInfo[code];
                var nameInFile = JsonSerializer.Serialize(katoFileInfo[code],options);
                var dictNameFromDb = JsonSerializer.Deserialize<Dictionary<string, string?>>(katoInfo.Name!);
                if (dictNameFromDb!["Ru"] != nameInDict["Ru"] )
                {
                    await _katoInfosRepository.UpdateName(code, nameInFile);
                }
            }
        }
    }
    public async Task Create(Dictionary<string, Dictionary<string, string>> katoFileInfo)
    {
        var codesFromDb = await _katoInfosRepository.GetCodesFromDb();
        foreach (var code in katoFileInfo.Keys.Where(code => !codesFromDb.Contains(code)))
        {
            await _katoInfosRepository.Create(code, katoFileInfo[code]);
        }
    }

    public async Task UpdateLocality(Dictionary<string, Dictionary<string, string>> katoFileInfo)
    {
        foreach (var code in katoFileInfo.Keys.Where(IsLocality))
        {
            await _katoInfosRepository.UpdateLocality(code);
        }
    }
}
    
