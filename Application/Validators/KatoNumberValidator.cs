using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class KatoNumberValidator : AbstractValidator<string>
{
    public KatoNumberValidator()
    {
        RuleFor(input => input)
            .NotEmpty().WithMessage("Input cannot be empty.")
            .Must(BeAllDigits).WithMessage("Input must consist of digits only.");
    }
    private bool BeAllDigits(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            foreach (char c in input)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }
}
