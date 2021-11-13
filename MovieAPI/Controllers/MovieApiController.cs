using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MovieAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MovieApiController : ControllerBase
    {
        private static List<MetaData> MetaDataList;
        private static List<MovieStatistic> MovieStatistics;

        public MovieApiController()
        {
            if (MetaDataList == null)
                MetaDataList = GetModelList_FromCsv<MetaData>(@"C:\metadata.csv");

            if (MovieStatistics == null)
                MovieStatistics = GetModelList_FromCsv<MovieStatistic>(@"C:\stats.csv");
        }

        private List<T> GetModelList_FromCsv<T>(string location) where T : Model
        {
            var modelList = new List<T>();
            using (var reader = new StreamReader(location))
            {
                reader.ReadLine(); //discard first line
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    var values1 = line.Split('"');
                    var values = new List<string>();

                    for (int i = 0; i < values1.Length; i++)
                    {
                        values1[i] = values1[i].Trim(',');

                        if (i % 2 == 0)
                            values.AddRange(values1[i].Split(','));
                        else
                            values.Add(values1[i]);
                    }

                    var model = GetModelFromStringArray<T>(values.ToArray());

                    modelList.Add(model);
                }
            }
            return modelList;
        }

        public T GetModelFromStringArray<T>(string[] values) where T : Model
        {
            Model model;

            if (typeof(T) == typeof(MetaData))
            {
                var id = int.Parse(values[0]);
                var movieId = int.Parse(values[1]);
                var title = values[2];
                var language = values[3];
                var duration = TimeSpan.Parse(values[4]);
                var releaseYear = int.Parse(values[5]);

                model = new MetaData()
                {
                    Id = id,
                    MovieId = movieId,
                    Title = title,
                    Language = language,
                    Duration = duration,
                    ReleaseYear = releaseYear
                };
            }
            else if (typeof(T) == typeof(MovieStatistic))
            {
                var movieId = int.Parse(values[0]);
                var watchDurationMs = int.Parse(values[1]);

                model = new MovieStatistic()
                {
                    MovieId = movieId,
                    WatchDurationMs = watchDurationMs
                };
            }
            else
                throw new Exception();

            return (T)model;
        }

        [HttpPost]
        [Route("metadata")]
        public IActionResult MetaData_Add([FromBody] MetaData metaData)
        {
            MetaDataList.Add(metaData);
            return Ok(metaData);
        }

        [HttpGet]
        [Route("metadata/{movieId:int}")]
        public IActionResult MetaData_GetByMovieId(int movieId)
        {
            var results = MetaDataList.Where(m => m.MovieId == movieId)
                                      .Where(m => m.GetType()
                                                   .GetProperties()
                                                   .All(p => !String.IsNullOrEmpty(p.GetValue(m)?.ToString() ?? "")))
                                      .GroupBy(m => m.Language)
                                      .Select(m => m.OrderByDescending(m1 => m1.Id).First())
                                      .OrderBy(m => m.Language)
                                      .ToList();

            if (results.Count == 0)
                return NotFound();

            return Ok(results);
        }

        [HttpGet]
        [Route("movies/stats")]
        public IActionResult MovieStats_GetAll()
        {
            var metaDataList = GetModelList_FromCsv<MetaData>(@"C:\metadata.csv");
            var results = metaDataList.GroupBy(m => m.MovieId)
                                      .Select(m => m.First())
                                      .Select(m => new 
            { 
                movieId = m.MovieId,
                title = m.Title,
                averageWatchDurationS = MovieStatistics.Where(ms => ms.MovieId == m.MovieId)
                                                       .Average(ms => ms.WatchDurationMs) / 1000,
                watches = MovieStatistics.Count(ms => ms.MovieId == m.MovieId),
                releaseYear = m.ReleaseYear
            })
                
                .OrderByDescending(m => m.watches)
                .ThenByDescending(m => m.releaseYear);

            return Ok(results);
        }
    }
}
