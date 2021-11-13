using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MovieAPI
{
    public class Repository
    {
        public List<MetaData> MetaDataList { get; set; }
        public List<MovieStatistic> MovieStatistics { get; set; }

        public Repository()
        {
            if (MetaDataList == null)
                MetaDataList = GetModelList_FromCsv<MetaData>("MovieAPI.Data.metadata.csv");

            if (MovieStatistics == null)
                MovieStatistics = GetModelList_FromCsv<MovieStatistic>("MovieAPI.Data.stats.csv");
        }

        public void MetaDataList_Add(MetaData model)
        {
            MetaDataList.Add(model);
        }

        public List<T> GetModelList_FromCsv<T>(string fileName) where T : Model
        {
            var modelList = new List<T>();
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName);
            using (var reader = new StreamReader(stream))
            {
                reader.ReadLine(); //discard first line as they are headers
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    var tempStrArray = line.Split('"');
                    var values = new List<string>();

                    for (int i = 0; i < tempStrArray.Length; i++)
                    {
                        tempStrArray[i] = tempStrArray[i].Trim(',');

                        if (i % 2 == 0)
                            values.AddRange(tempStrArray[i].Split(',')); //values with an even index are outside of speech marks and can be split by delimiter
                        else
                            values.Add(tempStrArray[i]); //don't split this by delimiter as it came from between speech marks
                    }

                    var model = GetModelFromStringArray<T>(values.ToArray());

                    modelList.Add(model);
                }
            }
            return modelList;
        }

        /// <summary>
        /// Generic method for converting an array of values from a row in a csv file to a chosen model type.
        /// If new models are to be used, code should be written to instruct how to parse the array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        private T GetModelFromStringArray<T>(string[] values) where T : Model
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
                throw new NotImplementedException("Parsing this model is not implemented");

            return (T)model;
        }
    }

}
