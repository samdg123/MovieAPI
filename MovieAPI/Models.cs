using System;

namespace MovieAPI
{
    public class Model { }
    public class MetaData : Model
    {
        public int Id { get; set; }

        public int MovieId { get; set; }

        public string Title { get; set; }

        public string Language { get; set; }
        public TimeSpan Duration { get; set; }
        public int ReleaseYear { get; set; }
    }

    public class MovieStatistic : Model
    {
        public int MovieId { get; set; }
        public int WatchDurationMs { get; set; }
    }
}
