using System.Collections.Generic;
using VideoStreamingAPI.Models;

namespace VideoStreamingAPI.Repositories
{
    public interface IMovieRepository
    {
        Task<Movie> GetMovieById(int id);
        Task<List<Movie>> GetAllMovies();
    }
}
