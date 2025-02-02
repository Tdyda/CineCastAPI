namespace VideoStreamingAPI.Models
{
    public class Actor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhotoPath{ get; set; }
        public virtual ICollection<MovieActor> MovieActors { get; set; }
    }
}
