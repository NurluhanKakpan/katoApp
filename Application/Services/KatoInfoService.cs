using Application.DTOs;
using Application.Validators;

namespace Application.Services;

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
        if (validatorResult.IsValid == false)
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
        var filteredCodes = GetLowLevelCodes(code, katoCodesFromDb);
        foreach (var t in filteredCodes)
        {
            var lowLevel = new LevelDto
            {
                Code = t,
                Name = await _katoInfosRepository.GetNameByCode(t)
            };
            listOfLowLevels.Add(lowLevel);
        }

        if (CheckKatoForLocality(code) == false)
        {
            var katoResultFalseDto = new KatoResulDto
            {
                Code = code,
                IsLocality = false,
                Locality = null,
                Region = await _katoInfosRepository.GetNameByCode(code.Substring(0, 2) + "0000000"),
                IsDeleted = (bool)katoInfoFromDb.IsDeleted!,
                HighLevel = highLevel,
                LowLevel = listOfLowLevels,
            };
            if (ParseKato(code) == "НП – районы в городе республиканского значения")
            {
                katoResultFalseDto.District = await _katoInfosRepository.GetNameByCode(code);
                katoResultFalseDto.LowLevel = listOfLowLevels;
            }
            else
            {
                katoResultFalseDto.District = await _katoInfosRepository.GetNameByCode(code.Substring(0, 4) + "00000");
            }
            return katoResultFalseDto;
        }
        var katoResultTrueDto = new KatoResulDto
        {
            Code = code,
            IsLocality = true,
            Locality = await _katoInfosRepository.GetNameByCode(code),
            Region = await _katoInfosRepository.GetNameByCode(code.Substring(0, 2) + "0000000"),
            District = await _katoInfosRepository.GetNameByCode(code.Substring(0, 4) + "00000"),
            IsDeleted = (bool)katoInfoFromDb.IsDeleted!,
            HighLevel = highLevel,
            LowLevel = listOfLowLevels,
        };
        if (katoResultTrueDto.Locality != katoResultTrueDto.Region) return katoResultTrueDto;
        katoResultTrueDto.District = null;
        katoResultTrueDto.HighLevel = null;
        return katoResultTrueDto;
    }

    private static string ParseKato(string katoNumber)
    {
        var ab = katoNumber.Substring(0, 2);
        var cd = katoNumber.Substring(2, 2);
        var ef = katoNumber.Substring(4, 2);
        var hij = katoNumber.Substring(6, 3);
        var ij = hij.Substring(1, 2);

        var abValue = int.Parse(ab);
        var cdValue = int.Parse(cd);
        var efValue = int.Parse(ef);
        var ijValue = int.Parse(ij);
        if (ijValue is > 2 and < 79 && int.Parse(katoNumber.Substring(6, 1)) > 1 &&
            int.Parse(katoNumber.Substring(6, 1)) < 9)
        {
            return ("Крестьянские и иные поселения с численностью жителей менее 50 человек");
        }

        if (ij == "01" || ij == "81" || ij == "91")
        {
            return ("выделенный резерв");
        }

        if (katoNumber.Substring(7, 1) is "0" or "8" or "9" && int.Parse(katoNumber.Substring(6, 1)) > 1 &&
            int.Parse(katoNumber.Substring(6, 1)) < 9)
        {
            return "Поселки Села с численностью жителей 50 и более человек";
        }

        if (efValue == 30)
        {
            return ("центр района, поселковой администрации или сельского округа");
        }

        if (int.Parse(katoNumber.Substring(6, 2)) == 10)
        {
            return ("города районного значения, центр района, поселковой администрации или сельского округа;");
        }

        if (efValue is > 30 and < 99)
        {
            return ("поселковая администрация или сельский округ, подчиненная(ый) объекту областного уровня");
        }

        if (efValue is >= 20 and < 30)
        {
            if (abValue is > 10 and < 70 && cdValue is > 10 and < 30)
            {
                return ("АТО районов в городе областного значения");
            }

            if (abValue is > 10 and < 70 && cdValue is >= 30 and < 80)
            {
                return ("АТО городов районного значения");
            }
        }

        if (efValue == 10)
        {
            if (abValue > 70 && cdValue is > 10 and < 20)
            {
                return ("НП – районы в городе республиканского значения");
            }

            if (abValue is > 10 and < 70 && cdValue is >= 10 and < 30)
            {
                return ("НП - города областного значения");
            }
        }

        if (cd == "00")
        {
            return ("АТО области или города республиканского значения");
        }

        if (cdValue == 10)
        {
            if (abValue is > 10 and < 70)
            {
                return ("АТО города областного значения");
            }
        }

        if (cdValue is > 10 and < 30)
        {
            return ("АТО районов в городе республиканского значения или НП - города областного значения");
        }

        if (cdValue is > 30 and <= 80)
        {
            return ("АТО районов");
        }

        if (abValue is > 10 and < 70)
        {
            return ("области");
        }

        if (abValue > 70)
        {
            return ("города республиканского значения");
        }

        return "Ваш вводимые данные не верный";
    }

    private static bool CheckKatoForLocality(string katoNumber)
    {
        if (ParseKato(katoNumber) == "Крестьянские и иные поселения с численностью жителей менее 50 человек")
        {
            return true;
        }

        if (ParseKato(katoNumber) == "НП - города областного значения")
        {
            return true;
        }

        if (ParseKato(katoNumber) == "АТО области или города республиканского значения")
        {
            return true;
        }

        if (ParseKato(katoNumber) == "Поселки Села с численностью жителей 50 и более человек")
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
        var highLevel = new LevelDto()
        {
            Code = GetHighLevelCode(code[..8],katoCodesFromDb),
            Name = await _katoInfosRepository.GetNameByCode(GetHighLevelCode(code[..8],katoCodesFromDb))
        };
        if (int.Parse(code[^1].ToString()) > 0)
        {
            highLevel.Code = code[..8] + "0";
            highLevel.Name = await _katoInfosRepository.GetNameByCode(code[..8] + "0");
        }
        return highLevel;
    }

    private static List<string> GetLowLevelCodes(string code, List<string> katoCodesFromDb)
    {
        var newCode = code.TrimEnd('0');
        if (newCode.Length == 9)
        {
            return new List<string>();
        }
        if (newCode.Length % 2 ==  1)
        {
            newCode += "0";
        }
        List<string> filteredCodes;
        if (newCode.Length == 8)
        {
            filteredCodes = katoCodesFromDb.Where(s => s.StartsWith(newCode)).ToList();
            return filteredCodes;
        }
        if (int.Parse(newCode) > 70 && newCode.Length == 2)
        {
            filteredCodes = katoCodesFromDb.Where(s => s.StartsWith(newCode)).ToList();
            return filteredCodes;
        }
        filteredCodes = katoCodesFromDb.Where(s => s.StartsWith(newCode) && int.Parse(s.Substring(newCode.Length, 2)) > 0 && IsZeroOtherLetters(s,newCode.Length + 2)).ToList();
        return filteredCodes;
    }
    private static bool IsZeroOtherLetters(string code, int n)
    {
        var newCode = code.Substring(n, 9-n);
        return newCode.All(t => t == '0');
    }
}
    
