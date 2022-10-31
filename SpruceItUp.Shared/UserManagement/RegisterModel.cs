using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace SpruceItUp.Shared.UserManagement
{
    public class RegisterModel
    {
        public bool _isHashed;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        [PasswordPropertyText] public string Password { get; set; }
        public AccountType Type { get; set; }
        public bool ModelsIsNull()
        {
            return FirstName == null || LastName == null || Email == null;
        }
    }
    public enum AccountType
    {
        [Display(Name = "Individual")] Individual,
        [Display(Name = "Organization")] Organization
    }
}