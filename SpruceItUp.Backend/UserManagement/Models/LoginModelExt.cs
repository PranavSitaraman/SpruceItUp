using SpruceItUp.Shared.UserManagement;
using UserManagement.Models;
namespace SpruceItUp.Backend.UserManagement.Models
{
    public static class LoginModelExt
    {
        public static EmailPassCredential GetCredential(this LoginModel model)
        {
            return new EmailPassCredential(null!, model.Email, model.Password);
        }
    }
}