using AngleSharp;
using AutoMapper;
using FetchPlatformData.Conditions.News;
using FetchPlatformData.Models;
using FetchPlatformData.Models.News;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FetchPlatformData.Services.News
{
    /// <summary>
    /// 蘋果日報
    /// </summary>
    public class AppledailyNewsService : IFetchData<AppledailyNewsConditions, Saver>
    {
        public async Task FetchDataAsync(AppledailyNewsConditions conditions, Saver saver)
        {

            var PlatformUrl = "https://tw.appledaily.com/";
            var client = new HttpClient();

            //maper config
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ChinatimesNewsModel, NewsDataModel>();
            });
            var mapper = mapperConfig.CreateMapper();

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);

            var hrefs = new List<string>();

            List<string> NewsURLList = new List<string>();

            for (DateTime day = DateTime.Now.Add(conditions.timeSpan); day < DateTime.UtcNow; day = day.AddDays(1))
            {
                //昔日文章
                var dailyurl = $"{PlatformUrl}archive/{day.ToString("yyyyMMdd")}/";
                //find post url
                var responseMessage = await client.GetAsync(dailyurl);
                var responseResult = await responseMessage.Content.ReadAsStringAsync();
                var document = await context.OpenAsync(res => res.Content(responseResult));
                var paginationHrefs = document.QuerySelectorAll(".archive-story").Select(x => x.GetAttribute("href"));
                foreach (var href in paginationHrefs)
                {
                    NewsURLList.Add(PlatformUrl + href);
                }
            }

            foreach (var href in NewsURLList)
            {
                var responseMessage = await client.GetAsync(href);
                var responseResult = await responseMessage.Content.ReadAsStringAsync();
                var document = await context.OpenAsync(res => res.Content(responseResult));

                var title = string.Join('\n', document.QuerySelectorAll(".text_medium").Select(x => x.TextContent));

                var contentItems = document.QuerySelectorAll("p").Select(x => x.TextContent);
                var content = string.Join('\n', contentItems);
                var postDate = href.Split('/')[4];
                if (content.IndexOf(conditions.Keyword) > 0)
                {
                    var model = new AppledailyNewsModel
                    {
                        Title = title,
                        Content = content,
                        Date = postDate,
                        Source = href
                    };

                    // save result
                    var result = mapper.Map<NewsDataModel>(model);
                    saver.Save(result);
                }
            }

            return;
        }
    }
}
