namespace VideoStreamingAPI.Models
{
    public class MovieTag
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
