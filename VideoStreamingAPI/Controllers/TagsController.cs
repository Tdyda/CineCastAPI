using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Formats.Asn1;
using VideoStreamingAPI.Data;
using VideoStreamingAPI.Models;

namespace VideoStreamingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TagsController : ControllerBase
    {
        private readonly VideoStreamingDbContext _context;

        public TagsController(VideoStreamingDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "admin")]

        [HttpPost("Add-Tag")]
        public async Task<ActionResult<Tag>> AddTag(string name)
        {
            var tag = new Tag()
            {
                Name = name                
            };

            if (ModelState.IsValid)
            {
                await _context.Tags.AddAsync(tag);
                await _context.SaveChangesAsync();
                return Ok(tag);
            }
            return BadRequest("Tag model is bad");
        }

        [HttpGet("Get-Tags")]
        public async Task<ActionResult<List<Tag>>> GetTags()
        {
            var tags = await _context.Tags.ToListAsync();

            if (tags.Count > 0)
            {
                return tags;
            }

            return BadRequest("Tags list was empty!");
        }
        [Authorize(Roles = "admin")]

        [HttpPost("Update-Tag")]
        public async Task<IActionResult> UpdateTag(int id, string name)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(x => x.Id == id);

            if (tag != null)
            {
                tag.Name = name;

                _context.Tags.Update(tag);
                await _context.SaveChangesAsync();

                return Ok(tag);
            }

            return NotFound("Tag wasn't find or had null values");
        }
        [Authorize(Roles = "admin")]

        [HttpDelete("Delete-Tag")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(x => x.Id == id);

            if (tag != null)
            {
                _context.Tags.Remove(tag);
                await _context.SaveChangesAsync();
                return Ok(tag);
            }

            return NotFound("Tag wasn't find or had null values");
        }
    }
}
