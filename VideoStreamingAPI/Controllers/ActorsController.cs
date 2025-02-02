using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using VideoStreamingAPI.Data;
using VideoStreamingAPI.Models;
using VideoStreamingAPI.Services;

namespace VideoStreamingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ActorsController : ControllerBase
    {
        private readonly VideoStreamingDbContext _context;
        private readonly string _actorsPhotosPath;
        private readonly IFileUploadService _fileUploadService;
        private readonly IFileRemoveService _fileRemoveService;

        public ActorsController(VideoStreamingDbContext context, IOptions<AppSettings> appSettings, IFileUploadService fileUploadService, IFileRemoveService fileRemoveService)
        {
            _context = context;
            _actorsPhotosPath = appSettings.Value.ActorsPhotosPath;
            _fileUploadService = fileUploadService;
            _fileRemoveService = fileRemoveService;
        }

        [HttpGet("Get-Actors")]
        public async Task<ActionResult<IEnumerable<Actor>>> GetActors()
        {
            var actors = await _context.Actors.ToListAsync();
            return Ok(actors);
        }
        [Authorize(Roles = "admin")]
        [HttpPost("Add-Actor")]
        public async Task<IActionResult> AddActor(IFormFile photo, [FromForm] string name)
        {
            var isUploaded = await _fileUploadService.UploadPhoto(Path.Combine(_actorsPhotosPath, name), photo);

            if (isUploaded)
            {
                var path = Path.Combine(_actorsPhotosPath, name, photo.FileName);
                var actor = new Actor()
                {
                    Name = name,
                    PhotoPath = path
                };

                if (ModelState.IsValid)
                {
                    await _context.Actors.AddAsync(actor);
                    _context.SaveChanges();
                    return Ok(actor);
                }
            }
            return BadRequest("Something went wrong, try again");
        }
        [Authorize(Roles = "admin")]
        [HttpDelete("Delete-Actor")]
        public async Task<IActionResult> DeleteActor(int id)
        {
            var actor = await _context.Actors.FirstOrDefaultAsync(x => x.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            var isRemoved = _fileRemoveService.RemoveFile(Path.Combine(_actorsPhotosPath, actor.Name));

            if (isRemoved)
            {
                _context.Actors.Remove(actor);
                await _context.SaveChangesAsync();

                return Ok(actor);
            }

            return BadRequest("Problem with remove file");
        }
        [Authorize(Roles = "admin")]
        [HttpPost("Edit-Actor")]
        public async Task<IActionResult> EditActor(int id, string name, IFormFile? photo = null)
        {
            var actor = await _context.Actors.FirstOrDefaultAsync(x => x.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            var oldPath = actor.PhotoPath;
            var fileName = oldPath.Split('\\').Last();

            if (photo != null)
            {
                var isUploaded = await _fileUploadService.UploadPhoto(Path.Combine(_actorsPhotosPath, actor.Name), photo);
                fileName = photo.FileName;
            }

            var isRename = _fileUploadService.RenameFile(Path.Combine(_actorsPhotosPath, actor.Name), Path.Combine(_actorsPhotosPath, name));

            if (isRename)
            {
                actor.Name = name;
                actor.PhotoPath = Path.Combine(_actorsPhotosPath, name, fileName);

                _context.Actors.Update(actor);
                await _context.SaveChangesAsync();

                return Ok(actor);
            }

            return BadRequest("Something went wrong");
        }
    }
}
