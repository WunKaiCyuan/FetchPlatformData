using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using FetchPlatformData.Conditions.News;
using FetchPlatformData.Models.News;
using FetchPlatformData.Services;
using FetchPlatformData.Services.News;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace FetchPlatformData
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var result = new List<NewsDataModel>();
            var keyword = string.Empty;

            foreach (var value in args)
            {
                if (value.StartsWith("--keyword="))
                    keyword = value.Substring("--keyword=".Length);
            }

            if (string.IsNullOrEmpty(keyword))
            {
                throw new ArgumentException("keyword is required");
            }

            // AutoMapper工具
            var mapperConfig = new MapperConfiguration(cfg=> {
                cfg.CreateMap<ChinatimesNewsModel, NewsDataModel>();
                cfg.CreateMap<LTNNewsModel, NewsDataModel>();
            });
            var mapper = mapperConfig.CreateMapper();

            // 中國時報
            var chinatimesNewsModel = await new ChinatimesNewsService().FetchDataAsync(new ChinatimesNewsConditions { Keyword = keyword });
            result.AddRange(mapper.Map<IEnumerable<NewsDataModel>>(chinatimesNewsModel));

            // 自由時報
            var ltnNewsModels = await new LTNNewsService().FetchDataAsync(new LTNNewsConditions { Keyword = keyword });
            result.AddRange(mapper.Map<IEnumerable<NewsDataModel>>(ltnNewsModels));

            // 產生CSV
            using (var writer = new StreamWriter($"{keyword}_result.csv"))
            {
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture) {
                    NewLine = Environment.NewLine,
                };

                using (var csv = new CsvWriter(writer, csvConfig))
                {
                    csv.WriteRecords(result);
                }
            }
        }
    }
}
