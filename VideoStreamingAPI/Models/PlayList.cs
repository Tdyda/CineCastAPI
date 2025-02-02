namespace VideoStreamingAPI.Models
{
    using System.Collections.Generic;

    namespace VideoStreamingAPI.Models
    {
        public class Playlist
        {
            public string FileName { get; set; }
            public List<VideoSegment> Segments { get; set; }
            public string SegmentsDirectory { get; set; }
        }
    }

}
