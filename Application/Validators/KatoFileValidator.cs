using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class KatoFileValidator : AbstractValidator<FileDataDto>
{
    public KatoFileValidator()
    {
        RuleFor(data => data.Code).NotEmpty().Length(9).Must(BeDigitsOnly)
            .WithMessage("Code must contain only digits.");
        RuleFor(data => data.RuName)
            .NotEmpty();
        RuleFor(data => data.KzName)
            .NotEmpty();
    }
    private static bool BeDigitsOnly(string code)
    {
        return code.All(char.IsDigit);
    }
}