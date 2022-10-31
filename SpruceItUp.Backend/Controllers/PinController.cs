using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentValidation.Results;
using LinqToDB;
using SpruceItUp.Backend.Hubs;
using SpruceItUp.Backend.Models;
using SpruceItUp.Shared;
using SpruceItUp.Shared.Models;
using SpruceItUp.Shared.Models.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using UserManagement.Models;
using LinqToDB.Data;
using Newtonsoft.Json;
using System.Security.Principal;
using System.Net.NetworkInformation;

namespace SpruceItUp.Backend.Controllers
{
    public class PinController : Controller
    {
        public const double TILE_SIZE = 0.01;
        private readonly AppDataConnection _connection;
        private readonly PinValidator _pinValidator;
        private readonly LocValidator _locValidator;
        private readonly IHubContext<PinHub, IPinClient> _pinHub;
        private readonly IHubContext<CommentHub, ICommentClient> _commentHub;
        public PinController([FromServices] AppDataConnection connection, [FromServices] IHubContext<PinHub, IPinClient> pinHubCtx, [FromServices] IHubContext<CommentHub, ICommentClient> commentHubCtx)
        {
            _connection = connection;
            _pinHub = pinHubCtx;
            _commentHub = commentHubCtx;
            _pinValidator = new();
            _locValidator = new();
        }
        [Route("Pin/Create")] [Authorize(Policy = "UserOnly")] [HttpPost] public async Task<IActionResult> CreatePin([FromBody] DbPin pin)
        {
            Guid.TryParse(User.FindFirstValue(ClaimTypes.Sid), out Guid userId);
            if (userId == Guid.Empty) return BadRequest();
            pin.Id = Guid.NewGuid();
            pin.Author = userId;
            ValidationResult result = await _pinValidator.ValidateAsync(pin);
            if (!result.IsValid) return BadRequest();
            await _connection.InsertAsync(pin);
            string groupName = PinHub.GetGroupName(pin.Position.FloorRound());
            await _pinHub.Clients.Group(groupName).ReceivePin(pin);
            return Ok();
        }
        [Route("Pin/Event")] [Authorize(Policy = "UserOnly")] [HttpPost] public async Task<IActionResult> CreateEvent([FromBody] DbLoc loc)
        {
            Guid.TryParse(User.FindFirstValue(ClaimTypes.Sid), out Guid userId);
            if (userId == Guid.Empty) return BadRequest();
            loc.Id = Guid.NewGuid();
            var id = loc.Id;
            loc.Author = userId;
            ValidationResult result = await _locValidator.ValidateAsync(loc);
            if (!result.IsValid) return BadRequest();
            foreach (var i in loc.Pins)
            {
                await _connection.Comments.Where(comment => comment.Pin == Guid.Parse(i)).DeleteAsync();
                await _connection.Pins.Where(pin => pin.Id == Guid.Parse(i)).DeleteAsync();
            }
            await _connection.InsertAsync(loc);
            return Ok(id);
        }
        [Route("Pin/UploadImage")] [Authorize(Policy = "UserOnly")] [HttpPost] public async Task<IActionResult> UploadImage()
        {
            var file = Request.Body;
            var folderName = Path.Combine("StaticFiles", "Images");
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            var fileName = $"IMG-{Guid.NewGuid()}.png";
            var fullPath = Path.Combine(pathToSave, fileName);
            var dbPath = "StaticFiles/Images/" + fileName;
            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return Ok(dbPath);
        }
        [Route("Pin/GetSurrounding")] [Authorize(Policy = "UserOnly")] [HttpGet] public async Task<IActionResult> GetSurroundingPins([FromQuery] double lat, [FromQuery] double lon)
        {
            List<Pin> pins = await _connection.Pins.Where(pin =>
                (pin.Lat - lat) * (pin.Lat - lat) + (pin.Lon - lon) * (pin.Lon - lon) < 0.04 * 0.04 && 
                pin.Expires > DateTime.UtcNow
            ).DefaultIfEmpty().ToListAsync();
            return Json(pins);
        }
        [Route("Pin/GetLocPast/{ss}")] [Authorize(Policy = "UserOnly")] [HttpGet] public async Task<IActionResult> GetLocPast(string ss)
        {
            Guid[] ids = JsonConvert.DeserializeObject<Guid[]>(ss);
            List<Loc> locs = await _connection.Locs.Where(loc => ids.Contains(loc.Id) && loc.Expires <= DateTime.UtcNow).OrderByDescending(x => x.Expires).DefaultIfEmpty().ToListAsync();
            return Json(locs);
        }
        [Route("Pin/GetLocFuture/{ss}")] [Authorize(Policy = "UserOnly")] [HttpGet] public async Task<IActionResult> GetLocFuture(string ss)
        {
            Guid[] ids = JsonConvert.DeserializeObject<Guid[]>(ss);
            List<Loc> locs = await _connection.Locs.Where(loc => ids.Contains(loc.Id) && loc.Expires > DateTime.UtcNow).OrderBy(x => x.Expires).DefaultIfEmpty().ToListAsync();
            return Json(locs);
        }
        [Route("Pin/GetLocs")] [Authorize(Policy = "UserOnly")] [HttpGet] public async Task<IActionResult> GetLocs()
        {
            List<Loc> locs = await _connection.Locs.Where(loc => loc.Expires > DateTime.UtcNow).DefaultIfEmpty().ToListAsync();
            return Json(locs);
        }
        [Route("Pin/GetPins")] [Authorize(Policy = "UserOnly")] [HttpGet] public async Task<IActionResult> GetPins()
        {
            List<Pin> pins = await _connection.Pins.Where(pin => pin.Expires > DateTime.UtcNow).DefaultIfEmpty().ToListAsync();
            return Json(pins);
        }
        [Route("Pin/Comment")] [Authorize(Policy = "UserOnly")] [HttpPost] public async Task<IActionResult> PostComment([FromBody] DbComment comment)
        {
            Guid.TryParse(User.FindFirstValue(ClaimTypes.Sid), out Guid userId);
            if (userId == Guid.Empty) return BadRequest();
            comment.Created = DateTime.UtcNow;
            comment.Author = userId;
            comment.Id = Guid.NewGuid();
            await _connection.InsertAsync(comment);
            string groupName = CommentHub.GetGroupName(comment.Pin);
            await _commentHub.Clients.Group(groupName).ReceiveComment(comment);
            return Ok();
        }
        [Route("Pin/GetComments/{id}")] [Authorize(Policy = "UserOnly")] [HttpGet] public async Task<IActionResult> GetComments(Guid id)
        {
            List<DbComment> comments = await _connection.Comments.Where(x => x.Pin == id).OrderByDescending(x => x.Created).ToListAsync();
            return Json(comments);
        }
    }
}