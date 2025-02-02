using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VideoStreamingAPI.Data;
using VideoStreamingAPI.Models;
using VideoStreamingAPI.Repositories;

namespace VideoStreamingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize()]
    public class MoviesCatalogController : Controller
    {
        private readonly VideoStreamingDbContext _context;
        private readonly string _thumbnailDirectory;

        public MoviesCatalogController(VideoStreamingDbContext context, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _thumbnailDirectory = appSettings.Value.UploadFolderPath;
        }
        [HttpGet("get-videos")]
        public async Task<ActionResult<IEnumerable<Movie>>> GetMovies([FromQuery] string query = "", [FromQuery] int limit = 10, [FromQuery] int offset = 0)
        {
            var moviesQuery = _context.Movies.AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                moviesQuery = moviesQuery.Where(movie => movie.Title.Contains(query));
            }

            var movies = await moviesQuery.Skip(offset).Take(limit).ToListAsync();

            var totalMovies = await _context.Movies.CountAsync();

            return Ok(new { Total = totalMovies, Movies = movies });
        }

        [HttpGet("tags")]
        public async Task<ActionResult<IEnumerable<Movie>>> GetMoviesByTag(
     [FromQuery(Name = "tags[]")] List<string> tags,
     [FromQuery] int limit = 10,
     [FromQuery] int offset = 0,
     [FromQuery] string query = "")
        {
            var movies = await _context.Movies
                .Include(m => m.MovieTags)
                .Where(m => m.MovieTags.Any(mt => tags.Contains(mt.Tag.Name)))
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return Ok(movies);
        }


        [HttpGet("actors")]
        public async Task<ActionResult<IEnumerable<Movie>>> GetMoviesByActor(
            [FromQuery(Name = "actors[]")] List<string> actors,
            [FromQuery] int limit = 10,
            [FromQuery] int offset = 0,
            [FromQuery] string query = "")
        {

            var movies = await _context.Movies
            .Include(m => m.MovieActors)
            .Where(m =>  m.MovieActors.Any(mt => actors.Contains(mt.Actor.Name)))
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

            return Ok(movies);
        }

        [HttpGet("thumbnail/{folderName}/{fileName}")]
        public IActionResult GetThumbnail(string folderName, string fileName)
        {
            var filePath = Path.Combine(_thumbnailDirectory, folderName, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Thumbnail not found.");
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "image/jpeg");
        }

        [HttpGet("preview/{folderName}/{fileName}")]
        public IActionResult GetPreview(string folderName, string fileName)
        {
            var filePath = Path.Combine(_thumbnailDirectory, folderName, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Thumbnail not found.");
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "video/mp4");
        }
    }
}
