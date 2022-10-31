using FluentValidation;
namespace SpruceItUp.Shared.Models.Validators
{
    public class LocValidator : AbstractValidator<Loc>
    {
        public LocValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty();
            RuleFor(x => x.Author).NotNull().NotEmpty();
            RuleFor(x => x.Lat).NotNull().NotEmpty();
            RuleFor(x => x.Lng).NotNull().NotEmpty();
            RuleFor(x => x.Title).MaximumLength(255).NotEmpty().NotNull();
            RuleFor(x => x.Kind).IsInEnum().NotNull();
            RuleFor(x => x.Image).MaximumLength(1023);
            RuleFor(x => x.Description).MaximumLength(8191);
        }
    }
}