using Azure;

namespace VideoStreamingAPI.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string SegmentsDirectory { get; set; }
        public string PlaylistFileName { get; set; }
        public virtual ICollection<MovieTag> MovieTags { get; set; }
        public virtual ICollection<MovieActor> MovieActors { get; set; }
    }
}
