using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SpruceItUp.Backend.UserManagement.Models;
using LinqToDB;
using SpruceItUp.Shared.UserManagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement;
using UserManagement.DataAccess;
using UserManagement.Models;
using System.Data.Common;
using Microsoft.Extensions.Configuration;

namespace SpruceItUp.Backend.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserActionManager _userActionManager;
        public AccountController([FromServices] UserDataConnection connection)
        {
            _userActionManager = new(connection);
        }
        [HttpGet] [Route("Account/CheckLogin")] public async Task<IActionResult> CheckLogin()
        {
            Guid.TryParse(User.FindFirstValue(ClaimTypes.Sid), out Guid userId);
            return Json(userId != Guid.Empty);
        }
        [HttpGet] [Route("Account/UserType")] public async Task<IActionResult> UserType()
        {
            Guid.TryParse(User.FindFirstValue(ClaimTypes.Sid), out Guid userId);
            if (userId == Guid.Empty) return Json(-1);
            UserDataConnection _connection = _userActionManager.GetDb();
            List<User> users = await _connection.Users.Where(user => user.UserId == userId).DefaultIfEmpty().ToListAsync();
            return Json(users[0].Type);
        }
        [HttpPost] [Route("Account/AddSignup")] public async Task<IActionResult> AddSignup([FromBody] Guid id)
        {
            Guid.TryParse(User.FindFirstValue(ClaimTypes.Sid), out Guid userId);
            UserDataConnection _connection = _userActionManager.GetDb();
            List<User> user = await _connection.Users.Where(user => user.UserId == userId).DefaultIfEmpty().ToListAsync();
            List<Guid> edit = new List<Guid>(user[0].Signups);
            edit.Add(id);
            user[0].Signups = edit.ToArray();
            await _connection.InsertOrReplaceAsync(user[0]);
            return Ok();
        }
        [HttpPost] [Route("Account/RemoveSignup")] public async Task<IActionResult> RemoveSignup([FromBody] Guid id)
        {
            Guid.TryParse(User.FindFirstValue(ClaimTypes.Sid), out Guid userId);
            UserDataConnection _connection = _userActionManager.GetDb();
            List<User> user = await _connection.Users.Where(user => user.UserId == userId).DefaultIfEmpty().ToListAsync();
            List<Guid> edit = new List<Guid>(user[0].Signups);
            edit.Remove(id);
            user[0].Signups = edit.ToArray();
            await _connection.InsertOrReplaceAsync(user[0]);
            return Ok();
        }
        [HttpGet] [Route("Account/Signups")] public async Task<IActionResult> Signups()
        {
            Guid.TryParse(User.FindFirstValue(ClaimTypes.Sid), out Guid userId);
            UserDataConnection _connection = _userActionManager.GetDb();
            List<User> users = await _connection.Users.Where(user => user.UserId == userId).DefaultIfEmpty().ToListAsync();
            return Json(users[0].Signups);
        }
        [HttpPost] [Route("Account/Register")] public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            var user = registerModel.GetNewUser();
            var credential = registerModel.GetNewCredential(user);
            RegisterResult result = await _userActionManager.RegisterNewUser(user, credential);
            switch (result)
            {
                case RegisterResult.Success: return Ok();
                case RegisterResult.DatabaseConflict: return Conflict();
                case RegisterResult.ModelValidationError: return BadRequest();
                default: return Forbid();
            }
        }
        [HttpPost] [Route("Account/Login")] public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            Credential cred = new EmailPassCredential()
            {
                Kind = CredentialKind.EmailPassword,
                Identifier = model.Email,
                Secret = model.Password
            };
            var result = await _userActionManager.LoginUser(cred);
            switch (result)
            {
                case LoginResult.NotExists: return NoContent();
                case LoginResult.Failure: return BadRequest();
            }
            var successResult = result as LoginResult.Success;
            Debug.Assert(successResult != null, nameof(successResult) + " != null");
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Sid, successResult.User.UserId.ToString())
            };
            ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new(identity), new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(480)
            });
            return Ok();
        }
        [HttpGet] [Route("Account/EmailTaken")] public async Task<IActionResult> EmailTaken([FromQuery] string email)
        {
            var user = await _userActionManager.GetUserByEmail(email);
            return Json(!(user == null));
        }
        [HttpGet] [Route("Account/GetName")] public async Task<IActionResult> GetName()
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.Sid), out Guid userId))
            {
                return Json(null);
            }
            var user = await _userActionManager.GetUserById(userId);
            if (user == null)
            {
                return Json(null);
            }
            return Json(user.FriendlyName);
        }
        [Authorize("UserOnly")] [HttpGet] [Route("Account/GetName/{id}")] public async Task<IActionResult> GetName(Guid id)
        {
            var user = await _userActionManager.GetUserById(id);
            if (user == null)
            {
                return Json(null);
            }
            return Json(user.FriendlyName);
        }
    }
}