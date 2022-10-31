﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using LinqToDB;
using CryptoHelper;
using UserManagement.DataAccess;
using UserManagement.Models;
using System.Security.Claims;
using SpruceItUp.Shared.UserManagement;

namespace UserManagement
{
    public abstract partial record RegisterResult
    {
        public record Success(User User) : RegisterResult;
        public record DatabaseConflict() : RegisterResult;
        public record ModelValidationError(List<ValidationFailure> UserErrors, List<ValidationFailure> CredentialErrors) : RegisterResult;
    }
    public abstract partial record LoginResult
    {
        public record Failure() : LoginResult;
        public record NotExists() : LoginResult;
        public record Success(User User) : LoginResult;
    }
    public class UserActionManager
    {
        private readonly UserDataConnection _connection;
        private readonly Validator _validator;
        public UserActionManager(UserDataConnection connection)
        {
            _connection = connection;
            _validator = new(_connection);
        }
        public async Task<RegisterResult> RegisterNewUser(User user, Credential credential)
        {
            (ValidationResult userValid, ValidationResult credentialValid) = await _validator.ValidateAll(user, credential);
            if (userValid.IsValid && credentialValid.IsValid)
            {
                await _connection.InsertAsync(user);
                await _connection.InsertAsync(credential);
                return new RegisterResult.Success(user);
            }
            else
            {
                return new RegisterResult.ModelValidationError(userValid.Errors, credentialValid.Errors);
            }
        }
        public async Task<LoginResult> LoginUser(Credential credential)
        {
            {
                Credential? cred = await _connection.Credentials.FirstOrDefaultAsync(c =>
                    c.Identifier == credential.Identifier
                );
                if (cred == null)
                {
                    return new LoginResult.NotExists();
                }
            }
            Credential?[] idMatchCredentials = await _connection.Credentials.Where(c =>
                c.Kind == credential.Kind && c.Identifier == credential.Identifier
            ).ToArrayAsync();
            Credential? loginCredential = null;
            foreach (Credential c in idMatchCredentials)
            {
                if (Crypto.VerifyHashedPassword(c.Secret, credential.Secret))
                {
                    loginCredential = c;
                    break;
                }
            }
            if (loginCredential == null)
            {
                return new LoginResult.Failure();
            }
            User? user = await _connection.Users.FirstAsync(u => u.UserId == loginCredential.UserId);
            if (user == null) return new LoginResult.Failure();
            return new LoginResult.Success(user);
        }
        public async Task<User?> GetUserByEmail(string email)
        {
            User? user = await _connection.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user;
        }
        public async Task<User?> GetUserById(Guid guid)
        {
            User? user = await _connection.Users.FirstOrDefaultAsync(u => u.UserId == guid);
            return user;
        }
        public UserDataConnection GetDb() => _connection;
    }
}