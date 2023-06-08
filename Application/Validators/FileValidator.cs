using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Validators;

public static class FileValidator
{
    public static bool ValidateFile(IFormFile formFile)
    {
        var validator = new KatoFileValidator();

        using var reader = new StreamReader(formFile.OpenReadStream());
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var columns = line!.Split(';');

            if (columns.Length != 3)
                return false; 

            var fileData = new FileDataDto()
            {
                Code = columns[0],
                RuName = columns[1],
                KzName = columns[2]
            };
            var validationResult = validator.Validate(fileData);
            if (!validationResult.IsValid)
                return false;
        }
        return true;
    }
        
}