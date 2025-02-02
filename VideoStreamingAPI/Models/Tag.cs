namespace VideoStreamingAPI.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<MovieTag> MovieTags { get; set; }
    }
}