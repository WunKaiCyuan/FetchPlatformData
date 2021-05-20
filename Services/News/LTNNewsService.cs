using AngleSharp;
using AutoMapper;
using FetchPlatformData.Conditions.News;
using FetchPlatformData.Models;
using FetchPlatformData.Models.News;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FetchPlatformData.Services.News
{
    /// <summary>
    /// 自由時報
    /// </summary>
    public class LTNNewsService : IFetchData<LTNNewsConditions, Saver>
    {
        public async Task FetchDataAsync(LTNNewsConditions conditions, Saver saver)
        {
            //var result = new List<LTNNewsModel>();
            var client = new HttpClient();

            //maper config
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LTNNewsModel, NewsDataModel>();
            });
            var mapper = mapperConfig.CreateMapper();

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);

            var page = 1;
            var hrefs = new List<string>();
            var paginationDataTotal = 0;

            var startDate = DateTime.Now.Add(conditions.timeSpan).ToString("yyyyMMdd");
            var endDate = DateTime.Now.ToString("yyyyMMdd");

            do
            {
                var url = $"https://search.ltn.com.tw/list?keyword={conditions.Keyword}&start_time={startDate}&end_time={endDate}&sort=date&type=all&page={page}";
                var responseMessage = await client.GetAsync(url);
                var responseResult = await responseMessage.Content.ReadAsStringAsync();
                var document = await context.OpenAsync(res => res.Content(responseResult));
                var paginationHrefs = document.QuerySelectorAll(".Searchnews .tit").Select(x => x.GetAttribute("href"));
                paginationDataTotal = paginationHrefs.Count();
                hrefs.AddRange(paginationHrefs);

                //debug
                //if (hrefs.Count() > 20)
                //    paginationDataTotal = 0;

                page++;
            } while (paginationDataTotal > 0);


            foreach (var href in hrefs)
            {
                var responseMessage = await client.GetAsync(href);
                var responseResult = await responseMessage.Content.ReadAsStringAsync();
                var document = await context.OpenAsync(res => res.Content(responseResult));

                var title = document.QuerySelector(".content h1").TextContent;
                var contentItems = document.QuerySelectorAll(".content .text p").Where(x => !x.HasAttribute("style") && !x.HasAttribute("class")).Select(x => x.TextContent);
                var content = string.Join('\n', contentItems);

                DateTime postDate;
                string pattern = @"[0-9]{4}\/[0-9]{2}\/[0-9]{2}";
                DateTime.TryParse(Regex.Match(document.QuerySelector(".time").TextContent, pattern).Value, out postDate);

                var model = new LTNNewsModel
                {
                    Title = title,
                    Content = content,
                    Date = postDate.ToString("yyyyMMdd"),
                    Source = href
                };


                // save result
                var result = mapper.Map<NewsDataModel>(model);
                saver.Save(result);
            }

            return;
        }
    }
}
