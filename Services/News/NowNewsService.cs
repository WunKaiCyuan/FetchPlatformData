using AngleSharp;
using AutoMapper;
using FetchPlatformData.Conditions.News;
using FetchPlatformData.Models;
using FetchPlatformData.Models.ApiReturn;
using FetchPlatformData.Models.News;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FetchPlatformData.Services.News
{
    /// <summary>
    /// 今新聞
    /// </summary>
    public class NowNewsService : IFetchData<NowNewsConditions, Saver>
    {
        //domain
        static string Domain = $"https://www.nownews.com";
        public async Task FetchDataAsync(NowNewsConditions conditions, Saver saver)
        {
            var client = new HttpClient();

            //maper config
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<NowNewsModel, NewsDataModel>();
            });
            var mapper = mapperConfig.CreateMapper();

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var hrefs = new List<string>();

            var startDate = DateTime.Now.Add(conditions.timeSpan);
            var endDate = DateTime.Now;

            var lastPostId = "0";
            var keepGetApi = true;

            //第一頁
            {
                var PageUrl = $"https://www.nownews.com/search?name={conditions.Keyword}";
                var responseMessage = await client.GetAsync(PageUrl);
                var responseResult = await responseMessage.Content.ReadAsStringAsync();
                var document = await context.OpenAsync(res => res.Content(responseResult));

                hrefs.AddRange(document.QuerySelectorAll(".news-card__title-s a").Select(x => x.GetAttribute("href")));
                //抓第一篇ID
                lastPostId = document.QuerySelector("#txtPageNo").GetAttribute("value");
            }


            //之後抓最後一篇

            //以API抓內容
            do
            {
                var ApiUrl = $"https://www.nownews.com/nn-client/api/v1/client/search?name={conditions.Keyword}&id={lastPostId}";
                var responseMessage = await client.GetAsync(ApiUrl);
                var responseResult = await responseMessage.Content.ReadAsStringAsync();
                //var document = await context.OpenAsync(res => res.Content(responseResult));
                var newsRespond = JsonConvert.DeserializeObject<NownewsSearchResult>(responseResult);

                hrefs.AddRange(newsRespond.data.Where(x => x.getDate > startDate && x.getDate <= DateTime.Now)
                                                .Select(x => Domain + x.postOnlyUrl));

                lastPostId = newsRespond.data.OrderBy(x => x.sort).Last().id;
                //抓滿8篇再抓一次
                keepGetApi = (newsRespond.data.Count() >= 8 &&
                                newsRespond.data.OrderBy(x => x.sort)
                                                .Last().getDate > startDate);

                keepGetApi = hrefs.Count > 10 ? false : keepGetApi;

            } while (keepGetApi);


            //逐頁內容
            foreach (var href in hrefs)
            {
                var responseMessage = await client.GetAsync(href);
                var responseResult = await responseMessage.Content.ReadAsStringAsync();
                var document = await context.OpenAsync(res => res.Content(responseResult));

                var title = document.QuerySelector(".article-title").TextContent;
                var contentItems = document.QuerySelector("article");
                foreach (var c in contentItems.ChildNodes)
                    continue;//contentItems.RemoveChild(c);
                //var content = contentItems.TextContent;
                var newsContent = contentItems.ChildNodes.Where(x => x.NodeName == "#text").Select(x => x.TextContent);
                DateTime postDate;
                string pattern = @"[0-9]{4}-[0-9]{2}-[0-9]{2}";
                DateTime.TryParse(Regex.Match(document.QuerySelector(".time").TextContent, pattern).Value, out postDate);

                var model = new NewsDataModel
                {
                    Title = title,
                    Content = string.Join("", newsContent).Replace("\t", "").Replace("\n", "").Replace(" ", ""),
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
