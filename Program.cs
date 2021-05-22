using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using FetchPlatformData.Conditions.News;
using FetchPlatformData.Models;
using FetchPlatformData.Models.News;
using FetchPlatformData.Services;
using FetchPlatformData.Services.News;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace FetchPlatformData
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var result = new List<NewsDataModel>();
            var keyword = string.Empty;
            TimeSpan timespan = TimeSpan.FromDays(-1);

            keyword = "疫情";
            foreach (var value in args)
            {
                if (value.StartsWith("--keyword="))
                    keyword = value.Substring("--keyword=".Length);
                if (value.StartsWith("--days="))
                    timespan = TimeSpan.FromDays(-int.Parse(value.Substring("--days=".Length)));
            }
            //儲存器
            Saver saver = Saver.getSaver(keyword);

            if (string.IsNullOrEmpty(keyword))
            {
                throw new ArgumentException("keyword is required");
            }

            // 中國時報
            //await new ChinatimesNewsService().FetchDataAsync(new ChinatimesNewsConditions { Keyword = keyword, timeSpan = timespan }, saver);

            // 自由時報 被鎖未測
            //await new LTNNewsService().FetchDataAsync(new LTNNewsConditions { Keyword = keyword, timeSpan = timespan }, saver);

            //蘋果日報
            //await new AppledailyNewsService().FetchDataAsync(new AppledailyNewsConditions { Keyword = keyword, timeSpan = timespan }, saver);

            //今日報
            //await new NowNewsService().FetchDataAsync(new NowNewsConditions { Keyword = keyword, timeSpan = timespan }, saver);


            //工商時報
            //await new CteeNewsService().FetchDataAsync(new CteeNewsConditions { Keyword = keyword, timeSpan = timespan }, saver);



            Console.WriteLine("---------Done!");
            System.Diagnostics.Process.Start(@"C:\Windows\explorer.exe", saver.FileLocation);

        }
    }
}
