using System;
using CryptoHelper;
using SpruceItUp.Shared.UserManagement;
using UserManagement.Models;
using System.Collections.Generic;
namespace SpruceItUp.Backend.UserManagement.Models
{
    public static class RegisterModelExt
    {
        private static void HashPw(this RegisterModel model)
        {
            if (!model._isHashed)
            {
                model.Password = Crypto.HashPassword(model.Password);
            }
        }
        public static User GetNewUser(this RegisterModel model)
        {
            return new User(Guid.NewGuid(), $"{model.FirstName} {model.LastName}", model.Email, model.Type, DateTime.UtcNow, (new List<Guid>()).ToArray());
        }
        public static EmailPassCredential GetNewCredential(this RegisterModel model, User user)
        {
            model.HashPw();
            return new EmailPassCredential(user, model.Email, model.Password);
        }
    }
}