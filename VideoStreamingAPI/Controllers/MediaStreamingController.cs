using System.IO;
using Microsoft.AspNetCore.Mvc;
using VideoStreamingAPI.Repositories;
using VideoStreamingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;

namespace VideoStreamingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MediaStreamingController : ControllerBase
    {
        private readonly IMovieRepository _movieRepository;
        private readonly ILogger<MediaStreamingController> _logger;


        public MediaStreamingController(IMovieRepository movieRepository, ILogger<MediaStreamingController> logger)
        {
            _movieRepository = movieRepository;
            _logger = logger;
        }
        [HttpGet("playlist/{id}")]        
        public async Task<IActionResult> GetPlaylist(int id)
        {
            var movie = await _movieRepository.GetMovieById(id);
            if (movie == null)
            {
                return NotFound("Movie not found.");
            }

            var playlistPath = Path.Combine(Directory.GetCurrentDirectory(), movie.SegmentsDirectory, movie.PlaylistFileName);
            _logger.LogInformation($"requested path: {playlistPath}");
            if (!System.IO.File.Exists(playlistPath))
            {
                return NotFound("Playlist not found.");
            }

            var playlistContent = System.IO.File.ReadAllText(playlistPath);

            var baseUrl = $"https://doublecodestudio.pl:51821/videoService/api/MediaStreaming/segment/{id}/";
            playlistContent = playlistContent.Replace("output", baseUrl + "output");
            return Content(playlistContent, "application/vnd.apple.mpegurl");
        }

        [HttpGet("segment/{id}/{fileName}")]
        public async Task<IActionResult> GetSegment(int id, string fileName)
        {
            var movie = await _movieRepository.GetMovieById(id);
            if (movie == null)
            {
                return NotFound("Movie not found.");
            }

            if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".ts"))
            {
                return BadRequest("Invalid segment request.");
            }

            var segmentPath = Path.Combine(Directory.GetCurrentDirectory(), movie.SegmentsDirectory, fileName);
            _logger.LogInformation($"segmentPath: {segmentPath}");
            
            if (!System.IO.File.Exists(segmentPath))
            {
                return NotFound("Segment not found.");
            }

            var fileStream = new FileStream(segmentPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return new FileStreamResult(fileStream, "video/MP2T");
        }
    }
}
