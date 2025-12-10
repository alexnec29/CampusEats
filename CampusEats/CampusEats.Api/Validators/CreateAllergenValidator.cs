using FluentValidation;
using static CampusEats.Api.Features.Allergen.CreateAllergen; 

namespace CampusEats.Api.Validators; 

public class CreateAllergenValidator : AbstractValidator<CreateAllergenCommand>
{
    public CreateAllergenValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");
        
        RuleFor(x => x.Name)
            .MaximumLength(50).WithMessage("Name cannot exceed 50 characters.");
            
    }
}