using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using VideoStreamingAPI.Data;
using VideoStreamingAPI.Models;

namespace VideoStreamingAPI.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly VideoStreamingDbContext _context;

        public MovieRepository(VideoStreamingDbContext context)
        {
            _context = context;
        }

        public async Task<Movie> GetMovieById(int id)
        {
            return await _context.FindAsync<Movie>(id);
        }

        public async Task<List<Movie>> GetAllMovies()
        {
            return await _context.Movies.ToListAsync();
        }
    }
}
