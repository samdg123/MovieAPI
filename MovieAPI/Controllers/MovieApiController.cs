using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace MovieAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MovieApiController : ControllerBase
    {
        private static Repository Repo = new Repository();        

        #region Endpoints
        [HttpPost]
        [Route("metadata")]
        public IActionResult MetaData_Add([FromBody] MetaData metaData)
        {
            Repo.MetaDataList_Add(metaData);
            return Ok(metaData);
        }

        [HttpGet]
        [Route("metadata/{movieId:int}")]
        public IActionResult MetaData_GetByMovieId(int movieId)
        {
            var results = Repo.MetaDataList.Where(m => m.MovieId == movieId)
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
            var metaDataList = Repo.GetModelList_FromCsv<MetaData>("MovieAPI.Data.metadata.csv");
            var results = metaDataList.GroupBy(m => m.MovieId)
                                      .Select(m => m.First())
                                      .Select(m => new 
            { 
                movieId = m.MovieId,
                title = m.Title,
                averageWatchDurationS = Repo.MovieStatistics.Where(ms => ms.MovieId == m.MovieId)
                                                            .Average(ms => ms.WatchDurationMs) / 1000,
                watches = Repo.MovieStatistics.Count(ms => ms.MovieId == m.MovieId),
                releaseYear = m.ReleaseYear
            })                
                .OrderByDescending(m => m.watches)
                .ThenByDescending(m => m.releaseYear);

            return Ok(results);
        }
        #endregion
    }
}
