using FluentValidation;
using LinqToDB;
using UserManagement.DataAccess;
namespace UserManagement.Models.Validators
{
    internal class CredentialValidator : AbstractValidator<Credential>
    {
        public CredentialValidator(UserDataConnection connection)
        {
            RuleFor(x => x.Identifier).NotEmpty().NotNull();
            RuleFor(x => x.Secret).NotEmpty().NotNull();
            RuleFor(x => x.CredentialId).NotEmpty().NotNull().MustAsync(async (guid, token) =>
                await connection.Credentials.FirstOrDefaultAsync(c => c.CredentialId == guid) == null
            );
        }
    }
}