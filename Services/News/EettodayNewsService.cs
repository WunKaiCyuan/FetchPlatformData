using AngleSharp;
using AutoMapper;
using FetchPlatformData.Conditions.News;
using FetchPlatformData.Models;
using FetchPlatformData.Models.News;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FetchPlatformData.Services.News
{
    /// <summary>
    /// ETtoday 新聞雲
    /// </summary>
    public class EttodayNewsService : IFetchData<NewsConditions, Saver>
    {
        public async Task FetchDataAsync(NewsConditions conditions, Saver saver)
        {
            //var result = new List<ChinatimesNewsModel>();

            var client = new HttpClient();

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);

            //get html content
            var page = 1;
            var hrefs = new List<string>();
            var paginationDataTotal = 0;
            do
            {
                var url = $"https://www.ettoday.net/news_search/doSearch.php?keywords={conditions.Keyword}&daydiff=3&page={page}";
                var responseMessage = await client.GetAsync(url);
                var responseResult = await responseMessage.Content.ReadAsStringAsync();
                var document = await context.OpenAsync(res => res.Content(responseResult));
                var datetimePattern = "2[0-9]{3}-[0|1][0-9]-[0-2][0-9] [0-2][0-9]:[0-5][0-9]";
                var paginationHrefs = document.QuerySelectorAll(".archive.clearfix")
                    .Where(x => DateTime.Parse(Regex.Match(x.QuerySelector(".date").TextContent, datetimePattern).Value) > DateTime.Now.Add(conditions.timeSpan))
                    .Select(x => x.QuerySelector("h2 a").GetAttribute("href"));
                paginationDataTotal = paginationHrefs.Count();
                hrefs.AddRange(paginationHrefs);

                page++;
            } while (paginationDataTotal > 0);



            foreach (var href in hrefs)
            {

                var responseMessage = await client.GetAsync(href);
                var responseResult = await responseMessage.Content.ReadAsStringAsync();
                var document = await context.OpenAsync(res => res.Content(responseResult));

                var title = document.QuerySelector("h1.title").TextContent;

                var story = document.QuerySelector("[itemprop=articleBody]");
                //刪除圖文廣告外嵌
                foreach (var d in story.QuerySelectorAll("iframe,a,img"))
                {
                    story.QuerySelector("iframe,a,img").Remove();
                }
                //圖註解
                var contentItems = story.QuerySelectorAll("p")
                    .Where(x => x.TextContent.Length > 20 && !x.TextContent.StartsWith("▲")).Select(x => x.TextContent);
                var content = string.Join(string.Empty, contentItems);
                var postDate = DateTime.Parse(document.QuerySelector(".date").GetAttribute("datetime"));
                var post = new EttodayNewsModel
                {
                    Title = title,
                    Content = content,
                    Date = postDate.ToString("yyyyMMdd"),
                    Source = href
                };

                //save result
                //var result = mapper.Map<NewsDataModel>(model);
                saver.Save(post);

            }

            return;
        }
    }
}
